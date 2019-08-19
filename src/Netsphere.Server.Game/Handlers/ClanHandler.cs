using System;
using System.Linq;
using System.Threading.Tasks;
using ExpressMapper.Extensions;
using Netsphere.Network.Data.Club;
using Netsphere.Network.Message.Club;
using Netsphere.Server.Game.Rules;
using ProudNet;

namespace Netsphere.Server.Game.Handlers
{
    internal class ClanHandler
        : IHandle<ClubSearchReqMessage>, IHandle<ClubInfoReqMessage>, IHandle<ClubNameCheckReqMessage>,
          IHandle<ClubCreateReqMessage>
    {
        private readonly ClanManager _clanManager;

        public ClanHandler(ClanManager clanManager)
        {
            _clanManager = clanManager;
        }

        [Firewall(typeof(MustBeLoggedIn))]
        [Firewall(typeof(MustBeInClan))]
        public async Task<bool> OnHandle(MessageContext context, ClubInfoReqMessage message)
        {
            var session = context.GetSession<Session>();
            var plr = session.Player;
            var clan = plr.Clan;

            session.Send(new ClubInfoAckMessage
            {
                ClanId = clan.Id,
                ClanIcon = clan.Icon,
                ClanName = clan.Name,
                MemberCount = clan.Count,
                OwnerName = clan.Owner.Name,
                CreationDate = clan.CreationDate,
                Area = clan.Area,
                Activity = clan.Activity,
                Class = clan.Class,
                Description = clan.Description
            });
            return true;
        }

        [Firewall(typeof(MustBeLoggedIn))]
        public async Task<bool> OnHandle(MessageContext context, ClubSearchReqMessage message)
        {
            var session = context.GetSession<Session>();

            // TODO Better queries, pages and sorting

            var result = _clanManager
                .Where(x => x.Name.Contains(message.Query, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Map<Clan, ClubSearchResultDto>())
                .ToArray();

            session.Send(new ClubSearchAckMessage(result));
            return true;
        }

        [Firewall(typeof(MustBeLoggedIn))]
        [Firewall(typeof(MustBeInClan), Invert = true)]
        public async Task<bool> OnHandle(MessageContext context, ClubNameCheckReqMessage message)
        {
            context.Session.Send(new ClubNameCheckAckMessage(
                _clanManager.CheckClanName(message.Name)
            ));
            return true;
        }

        [Firewall(typeof(MustBeLoggedIn))]
        [Firewall(typeof(MustBeInClan), Invert = true)]
        public async Task<bool> OnHandle(MessageContext context, ClubCreateReqMessage message)
        {
            var session = context.GetSession<Session>();
            var plr = session.Player;

            if (_clanManager.CheckClanName(message.Name) != ClubNameCheckResult.Available)
                session.Send(new ClubCreateAckMessage(ClubCreateResult.Failed));

            var (clan, result) = await _clanManager.CreateClan(
                plr,
                message.Name, message.Description,
                message.Area, message.Activity,
                message.Question1, message.Question2, message.Question3, message.Question4, message.Question5
            );

            if (result == ClanCreateError.None)
            {
                session.Send(new ClubCreateAckMessage(ClubCreateResult.Success));
            }
            else
            {
                session.Send(new ClubCreateAckMessage(ClubCreateResult.Failed));
            }

            return true;
        }
    }
}
