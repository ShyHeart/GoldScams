using System.Collections.Generic;
using Protocol.Code;
using Protocol.Constant;
using Protocol.Dto;
using Protocol.Dto.Fight;

public class FightHandler : BaseHandler
{
    public override void OnReceive(int subCode, object value)
    {
        switch (subCode)
        {
            case FightCode.StartFight_BRO:
                StartFight_BRO(value as List<PlayerDto>);
                break;
            case FightCode.Leave_BRO:
                EventCenter.Broadcast(EventDefine.LeaveFightRoom, (int)value);
                break;
            case FightCode.StartStakes_BRO:
                EventCenter.Broadcast(EventDefine.StartStakes, (int)value);
                break;
            case FightCode.LookCard_BRO:
                EventCenter.Broadcast(EventDefine.LookCardBRO, (int)value);
                break;
            case FightCode.PutStakes_BRO:
                EventCenter.Broadcast(EventDefine.PutStakesBRO,(StakesDto)value);
                break;
            case FightCode.GiveUpCard_BRO:
                EventCenter.Broadcast(EventDefine.GiveUpCardBRO, (int)value);
                break;
            case FightCode.CompareCard_BRO:
                EventCenter.Broadcast(EventDefine.CompareCardBRO,(CompareResultDto)value);
                break;
            case FightCode.GameOver_BRO:
                EventCenter.Broadcast(EventDefine.GameOver,(GameOverDto)value);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 开始战斗的服务器广播
    /// </summary>
    /// <param name="playerList"></param>
    private void StartFight_BRO(List<PlayerDto> playerList)
    {
        foreach (var player in playerList)
        {
            //左边玩家
            if (player.userId == Models.GameModel.MatchRoomDto.LeftPlayerId)
            {
                //如果左边玩家是庄家，就广播庄家的事件码
                if (player.identity == Identity.Banker)
                {
                    EventCenter.Broadcast(EventDefine.LeftBanker);
                }
                EventCenter.Broadcast(EventDefine.LestDealCard, player);
            }
            //右边玩家
            if (player.userId == Models.GameModel.MatchRoomDto.RightPlayerId)
            {
                if (player.identity == Identity.Banker)
                {
                    EventCenter.Broadcast(EventDefine.RightBanker);
                }
                EventCenter.Broadcast(EventDefine.RightDealCard, player);
            }
            //自身玩家
            if (player.userId == Models.GameModel.userDto.UserId)
            {
                if (player.identity == Identity.Banker)
                {
                    EventCenter.Broadcast(EventDefine.SelfBanker);
                }

                EventCenter.Broadcast(EventDefine.SelfDealCard, player);
            }

        }
    }
}
