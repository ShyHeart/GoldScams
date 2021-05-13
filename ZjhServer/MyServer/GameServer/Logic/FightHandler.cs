using System;
using MyServer;
using System.Collections.Generic;
using GameServer.Fight;
using GameServer.Cache;
using GameServer.DataBase;
using MyServer.TimerTool;
using Protocol.Code;
using Protocol.Constant;
using Protocol.Dto;
using Protocol.Dto.Fight;

namespace GameServer.Logic
{
    public class FightHandler : IHandler
    {
        /// <summary>
        /// 战斗缓存
        /// </summary>
        private FightCache fightCache = Caches.fightCache;

        /// <summary>
        /// 下注的传输模型
        /// </summary>
        private StakesDto stakesDto = new StakesDto();
        /// <summary>
        /// 比牌结果传输模型
        /// </summary>
        private CompareResultDto resultDto = new CompareResultDto();
        /// <summary>
        /// 游戏结束传输模型
        /// </summary>
        private GameOverDto overDto = new GameOverDto();


        public void Disconnect(ClientPeer client)
        {
            LeaveRoom(client);
        }

        public void Receive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case FightCode.Leave_CREQ:
                    LeaveRoom(client);
                    break;
                case FightCode.LookCard_CREQ:
                    LookCard(client);
                    break;
                case FightCode.Follow_CREQ:
                    Follow(client);
                    break;
                case FightCode.AddStakes_CREQ:
                    AddStakes(client, (int)value);
                    break;
                case FightCode.GiveUpCard_CREQ:
                    GiveUpCard(client);
                    break;
                case FightCode.CompareCard_CREQ:
                    CompareCard(client, (int)value);
                    break;
                default:
                    break;
            }
        }

        #region 比牌逻辑

                /// <summary>
        /// 客户端比牌的处理
        /// </summary>
        /// <param name="compareClient">比较者</param>
        /// <param name="comparedUserId">被比较者Id</param>
        private void CompareCard(ClientPeer compareClient, int comparedUserId)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (fightCache.IsFighting(compareClient.Id) == false) return;
                FightRoom room = fightCache.GetFightRoomByUserId(compareClient.Id);
                room.StakesSum += room.LastPlayerStakesCount;
                int stakesSum = room.UpdatePlayerStakesSum(compareClient.Id, room.LastPlayerStakesCount);
                int remainCoin = DatabaseManager.UpdateCoinCount(compareClient.Id, -room.LastPlayerStakesCount);
                stakesDto.Change(compareClient.Id, remainCoin, room.LastPlayerStakesCount, stakesSum, StakesDto.StakesType.Look);
                room.Broadcast(OpCode.Fight, FightCode.PutStakes_BRO, stakesDto);

                PlayerDto compareDto = null, comparedDto = null, otherDto = null;
                //遍历 拿到比较者的Dto 被比较者的Dto 其他玩家的Dto
                foreach (var player in room.playerList)
                {
                    if (player.userId == compareClient.Id)
                    {
                        compareDto = player;
                    }
                    else if (player.userId == comparedUserId)
                    {
                        comparedDto = player;
                    }
                    else
                    {
                        otherDto = player;
                    }
                }
                ClientPeer otherClient = DatabaseManager.GetClientPeerByUserId(otherDto.userId);
                //比牌
                CompareCard(room,compareClient,compareDto,comparedDto,otherClient);
            });
        }
        /// <summary>
        /// 比牌的逻辑算法
        /// </summary>
        /// <param name="room">房间</param>
        /// <param name="compareClient">比牌者</param>
        /// <param name="compare">胜利的Dto</param>
        /// <param name="compareD">输了的</param>
        /// <param name="otherClient">其他的</param>
        private void CompareCard(FightRoom room, ClientPeer compareClient, PlayerDto compare, PlayerDto compareD, ClientPeer otherClient)
        {
            if (compare.cardType > compareD.cardType)
            {
                //比较者胜利
                CompareResult(room, compareClient, compare, compareD, otherClient);
            }
            else if (compare.cardType == compareD.cardType)
            {
                if (compare.cardType == CardType.Min)
                {
                    CompareCardWhenMin(room, compareClient, compare, compareD, otherClient);
                }

                //662 663 766 866
                if (compare.cardType == CardType.Duizi)
                {
                    int compareDuiziValue = 0, compareDanValue = 0, compareDduiziValue = 0, compareDdanValue = 0;
                    //比较者
                    if (compare.cardList[0].Weight == compare.cardList[1].Weight)
                    {
                        compareDuiziValue = compare.cardList[0].Weight;
                        compareDanValue = compare.cardList[2].Weight;
                    }
                    if (compare.cardList[1].Weight == compare.cardList[2].Weight)
                    {
                        compareDuiziValue = compare.cardList[1].Weight;
                        compareDanValue = compare.cardList[0].Weight;
                    }
                    //被比较者
                    if (compareD.cardList[0].Weight == compareD.cardList[1].Weight)
                    {
                        compareDduiziValue = compareD.cardList[0].Weight;
                        compareDdanValue = compareD.cardList[2].Weight;
                    }
                    if (compareD.cardList[1].Weight == compareD.cardList[2].Weight)
                    {
                        compareDduiziValue = compareD.cardList[1].Weight;
                        compareDdanValue = compareD.cardList[0].Weight;
                    }

                    if (compareDuiziValue > compareDduiziValue)
                    {
                        //比较者胜利
                        CompareResult(room, compareClient, compare, compareD, otherClient);
                    }
                    else if (compareDuiziValue == compareDduiziValue)
                    {
                        if (compareDanValue > compareDdanValue)
                        {
                            //比较者胜利
                            CompareResult(room, compareClient, compare, compareD, otherClient);
                        }
                        else
                        {
                            //比较者失败
                            CompareResult(room, compareClient, compareD, compare, otherClient);
                        }
                    }
                    else
                    {
                        //比较者失败
                        CompareResult(room, compareClient, compareD, compare, otherClient);
                    }
                }

                if (compare.cardType == CardType.Shunzi || compare.cardType == CardType.Shunjin ||
                    compare.cardType == CardType.Baozi)
                {
                    int compareSum = 0, compareDSum = 0;
                    for (int i = 0; i < compare.cardList.Count; i++)
                    {
                        compareSum += compare.cardList[i].Weight;
                    }
                    for (int i = 0; i < compareD.cardList.Count; i++)
                    {
                        compareDSum += compareD.cardList[i].Weight;
                    }

                    if (compareSum > compareDSum)
                    {
                        //比较者胜利
                        CompareResult(room, compareClient, compare, compareD, otherClient);
                    }
                    else
                    {
                        //比较者失败
                        CompareResult(room, compareClient, compareD, compare, otherClient);
                    }
                }

                if (compare.cardType == CardType.Jinhua)
                {
                    CompareCardWhenMin(room, compareClient, compare, compareD, otherClient);
                }

                if (compare.cardType == CardType.Max)
                {
                    //比较者失败
                    CompareResult(room, compareClient, compareD, compare, otherClient);
                }
            }
            else
            {
                //比较者失败
                CompareResult(room, compareClient, compareD, compare, otherClient);
            }
        }

        /// <summary>
        /// 单张牌型 金花牌型 的比牌逻辑
        /// </summary>
        /// <param name="room">房间</param>
        /// <param name="compareClient">比牌者</param>
        /// <param name="compare">胜利的Dto</param>
        /// <param name="compareD">输了的</param>
        /// <param name="otherClient">其他的</param>
        private void CompareCardWhenMin(FightRoom room, ClientPeer compareClient, PlayerDto compare, PlayerDto compareD, ClientPeer otherClient)
        {
            if (compare.cardList[0].Weight > compareD.cardList[0].Weight)
            {
                //比较者胜利
                CompareResult(room, compareClient, compare, compareD, otherClient);
            }
            else if (compare.cardList[0].Weight == compareD.cardList[0].Weight)
            {
                if (compare.cardList[1].Weight > compareD.cardList[1].Weight)
                {
                    //比较者胜利
                    CompareResult(room, compareClient, compare, compareD, otherClient);
                }
                else if (compare.cardList[1].Weight == compareD.cardList[1].Weight)
                {
                    if (compare.cardList[2].Weight > compareD.cardList[2].Weight)
                    {
                        //比较者胜利
                        CompareResult(room, compareClient, compare, compareD, otherClient);
                    }
                    else
                    {
                        //比较者失败
                        CompareResult(room, compareClient, compareD, compare, otherClient);
                    }
                }
                else
                {
                    //比较者失败
                    CompareResult(room, compareClient, compareD, compare, otherClient);
                }
            }
            else
            {
                //比较者失败
                CompareResult(room, compareClient, compareD, compare, otherClient);
            }
        }
       
        /// <summary>
        /// 比牌结果
        /// </summary>
        private void CompareResult(FightRoom room, ClientPeer compareClient, PlayerDto winDto, PlayerDto loseDto, ClientPeer otherClient)
        {
            //胜利的玩家，开始下注
            Turn(compareClient, winDto.userId);
            //输了的玩家 弃牌
            GiveUpCard(loseDto.userId);
            resultDto.Change(winDto, loseDto);

            Console.WriteLine("胜利者：" + winDto.userName + "  失败者：" + loseDto.userName);
            room.Broadcast(OpCode.Fight, FightCode.CompareCard_BRO, resultDto, otherClient);

        }


        #endregion

        #region 弃牌

        /// <summary>
        /// 客户端弃牌的请求处理
        /// </summary>
        /// <param name="client"></param>
        private void GiveUpCard(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (fightCache.IsFighting(client.Id) == false) return;

                FightRoom room = fightCache.GetFightRoomByUserId(client.Id);
                room.GiveUpCardUserIdList.Add(client.Id);
                room.Broadcast(OpCode.Fight, FightCode.GiveUpCard_BRO, client.Id);

                //离开的玩家是本次下注的玩家
                //这样的话，需要转换下一个2玩家下注 
                if (room.RoundModel.CurrentStakesUserId == client.Id)
                {
                    //轮换下注 
                    Turn(client);
                }

                if (room.GiveUpCardUserIdList.Count >= 1 && room.leaveUserIdList.Count >= 1)
                {
                    GameOver(room);
                }
                //如果弃牌的人数为2
                if (room.GiveUpCardUserIdList.Count == 2)
                {
                    GameOver(room);

                }
            });
        }
        private void GiveUpCard(int userId)

        {
            SingleExecute.Instance.Execute(() =>
            {
                if (fightCache.IsFighting(userId) == false) return;

                FightRoom room = fightCache.GetFightRoomByUserId(userId);
                room.GiveUpCardUserIdList.Add(userId);
                room.Broadcast(OpCode.Fight, FightCode.GiveUpCard_BRO, userId);

                if (room.GiveUpCardUserIdList.Count >= 1 && room.leaveUserIdList.Count >= 1)
                {
                    GameOver(room);
                }

                //如果弃牌的人数为2
                if (room.GiveUpCardUserIdList.Count == 2)
                {
                    GameOver(room);

                }
            });
        }


        #endregion

        #region 加注 跟注 看牌 下注

        /// <summary>
        /// 客户端加注的处理
        /// </summary>
        /// <param name="client"></param>
        /// <param name="multiple"></param>
        private void AddStakes(ClientPeer client, int multiple)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (fightCache.IsFighting(client.Id) == false) return;

                FightRoom room = fightCache.GetFightRoomByUserId(client.Id);
                room.LastPlayerStakesCount *= multiple;
                room.StakesSum += room.LastPlayerStakesCount;
                if (room.LastPlayerStakesCount > room.TopStakes)
                {
                    room.LastPlayerStakesCount = room.TopStakes;
                }
                room.StakesSum += room.LastPlayerStakesCount;
                int stakesSum = room.UpdatePlayerStakesSum(client.Id, room.LastPlayerStakesCount);
                int remainCoin = DatabaseManager.UpdateCoinCount(client.Id, -room.LastPlayerStakesCount);
                stakesDto.Change(client.Id, remainCoin, room.LastPlayerStakesCount, stakesSum, StakesDto.StakesType.NoLook);
                room.Broadcast(OpCode.Fight, FightCode.PutStakes_BRO, stakesDto);

                if (remainCoin < room.LastPlayerStakesCount)
                {
                    GiveUpCard(client);
                    return;
                }

                Turn(client);

            });
        }

        /// <summary>
        /// 客户端跟注请求的处理
        /// </summary>
        /// <param name="client"></param>
        private void Follow(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (fightCache.IsFighting(client.Id) == false) return;

                FightRoom room = fightCache.GetFightRoomByUserId(client.Id);
                room.StakesSum += room.LastPlayerStakesCount;
                var stakesSum = room.UpdatePlayerStakesSum(client.Id, room.LastPlayerStakesCount);
                var remainCoin = DatabaseManager.UpdateCoinCount(client.Id, -(room.LastPlayerStakesCount));
                stakesDto.Change(client.Id, remainCoin, room.LastPlayerStakesCount, stakesSum, StakesDto.StakesType.NoLook);
                room.Broadcast(OpCode.Fight, FightCode.PutStakes_BRO, stakesDto);

                if (remainCoin < room.LastPlayerStakesCount)
                {
                    GiveUpCard(client);
                    return;
                }

                //轮到下一个玩家下注
                Turn(client);
            });
        }

        /// <summary>
        /// 客户端看牌请求的处理
        /// </summary>
        /// <param name="client"></param>
        private void LookCard(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (fightCache.IsFighting(client.Id) == false) return;

                FightRoom room = fightCache.GetFightRoomByUserId(client.Id);
                room.Broadcast(OpCode.Fight, FightCode.LookCard_BRO, client.Id);
            });
        }

        private ClientPeer timerClient;
        /// <summary>
        /// 轮换下注的处理
        /// </summary>
        /// <param name="client"></param>
        private void Turn(ClientPeer client, int turnUserId = -1)
        {
            //清空一下定时器
            TimerManager.Instance.Clear();
            if (fightCache.IsFighting(client.Id) == false) return;

            FightRoom room = fightCache.GetFightRoomByUserId(client.Id);
            var nextId = room.Turn();

            if (room.IsGiveUpCard(nextId) || room.IsLeaveRoom(nextId) || (turnUserId != -1 && nextId != turnUserId))
            {
                //如果下一位玩家离开或者弃牌了，就继续轮换下注，直到该玩家不离开也不弃牌
                Turn(client, turnUserId);
            }
            else
            {
                timerClient = DatabaseManager.GetClientPeerByUserId(nextId);
                //添加定时器任务
                TimerManager.Instance.AddTimerEvent(60, TimerDelegate);
                Console.WriteLine("当前下注者：" + client.UserName);
                room.Broadcast(OpCode.Fight, FightCode.StartStakes_BRO, nextId);
            }
        }


        #endregion

        /// <summary>
        /// 计时器执行的任务
        /// </summary>
        private void TimerDelegate()
        {
            //倒计时结束
            //跟注处理
            Follow(timerClient);
        }
        /// <summary>
        /// 客户端离开请求的处理
        /// </summary>
        /// <param name="client"></param>
        private void LeaveRoom(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                //不在战斗房间，忽略
                if (fightCache.IsFighting(client.Id) == false) return;

                FightRoom room = fightCache.GetFightRoomByUserId(client.Id);
                room.leaveUserIdList.Add(client.Id);

                //离开房间的惩罚  广播一下有人离开了
                DatabaseManager.UpdateCoinCount(client.Id, -(room.BottomStakes * 20));
                room.Broadcast(OpCode.Fight, FightCode.Leave_BRO, client.Id);

                //离开的玩家是本次下注的玩家
                //这样的话，需要转换下一个2玩家下注 
                if (room.RoundModel.CurrentStakesUserId == client.Id)
                {
                    //轮换下注 
                    Turn(client);
                }
                if (room.GiveUpCardUserIdList.Count >= 1 && room.leaveUserIdList.Count >= 1)
                {
                    GameOver(room);
                }

                //如果房间里=2，说明有人离开，游戏结束
                if (room.leaveUserIdList.Count == 2)
                {
                    GameOver(room);
                    return;
                }
                //如果房间离开人数等于3，就将房间销毁
                if (room.leaveUserIdList.Count == 3)
                {
                    fightCache.DestoryRoom(room);
                }

            });
        }

        /// <summary>
        /// 游戏结束
        /// </summary>
        /// <param name="room"></param>
        private void GameOver(FightRoom room)
        {
            PlayerDto winPlayer = null;
            List<PlayerDto> loseList = new List<PlayerDto>();

            //遍历玩家列表 判断谁赢谁输
            foreach (var player in room.playerList)
            {
                //胜利的玩家
                //通过判断谁不在弃牌列表 并且不在离开列表里面 满足这两个条件，结束胜利的玩家
                if (!room.IsGiveUpCard(player.userId) && !room.IsLeaveRoom(player.userId))
                {
                    winPlayer = player;
                }
                else
                {
                    loseList.Add(player);
                }
            }
            //更新金币数量
            DatabaseManager.UpdateCoinCount(winPlayer.userId, room.StakesSum);
            //传入赢的 输的 赢的数量
            overDto.Change(winPlayer, loseList, room.StakesSum);
            
            room.Broadcast(OpCode.Fight, FightCode.GameOver_BRO, overDto);
            //销毁房间
            fightCache.DestoryRoom(room);
            //清空定时任务
            TimerManager.Instance.Clear();

        }
        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="clientList"></param>
        /// <param name="roomType"></param>
        public void StatrFight(List<ClientPeer> clientList, int roomType)
        {
            SingleExecute.Instance.Execute(() =>
            {
                FightRoom room = fightCache.CreatRoom(clientList);
                switch (roomType)
                {
                    case 0:
                        room.BottomStakes = 10;
                        room.TopStakes = 100;
                        room.LastPlayerStakesCount = 10;
                        break;
                    case 1:
                        room.BottomStakes = 20;
                        room.TopStakes = 200;
                        room.LastPlayerStakesCount = 20;
                        break;
                    case 2:
                        room.BottomStakes = 50;
                        room.TopStakes = 500;
                        room.LastPlayerStakesCount = 50;
                        break;
                    default:
                        break;
                }

                foreach (var client in clientList)
                {
                    room.UpdatePlayerStakesSum(client.Id, room.BottomStakes);
                }
                //选择庄家
                ClientPeer bankerClient = room.SetBanker();
                //重置位置，排序
                room.ResetPosition(bankerClient.Id);
                //发牌
                room.DealCards();
                //对手牌排序
                room.SortAllPlayerCard();
                //获得牌型
                room.GetAllPlayerCardType();

                room.Broadcast(OpCode.Fight, FightCode.StartFight_BRO, room.playerList);
                //转换下注，换到庄家下一位玩家下注
                Turn(bankerClient);
            });
        }
    }
}
