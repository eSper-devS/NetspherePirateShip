using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Netsphere.Database;
using Netsphere.Database.Auth;
using Netsphere.Database.Game;

namespace Netsphere.Server.Game
{
    public class ClanManager : IHostedService, IReadOnlyCollection<Clan>
    {
        private readonly ILogger _logger;
        private readonly DatabaseService _databaseService;
        private ImmutableDictionary<uint, Clan> _clans;

        public Clan this[uint id] => GetClan(id);
        public Clan this[string name] => GetClan(name);

        public ClanManager(ILogger<ClanManager> logger, DatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        public Clan GetClan(uint id)
        {
            return _clans.GetValueOrDefault(id);
        }

        public Clan GetClan(string name)
        {
            return _clans.Values.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Loading clans...");

            using (var db = _databaseService.Open<GameContext>())
            using (var authDb = _databaseService.Open<AuthContext>())
            {
                var clanEntities = await db.Clans
                    .Include(x => x.Members)
                    .ToArrayAsync();

                var clans = new List<Clan>();
                foreach (var clanEntity in clanEntities)
                {
                    var memberIds = clanEntity.Members.Select(x => x.PlayerId).ToList();
                    var accounts = await authDb.Accounts
                        .Where(x => memberIds.Contains(x.Id))
                        .ToArrayAsync();

                    clans.Add(new Clan(
                        clanEntity,
                        clanEntity.Members.Select(x => (x, accounts.First(acc => acc.Id == x.PlayerId)))
                    ));
                }

                _clans = clans.ToImmutableDictionary(x => x.Id, x => x);
            }

            _logger.Information("Loaded {Count} clans", _clans.Count);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #region IReadOnlyCollection
        public int Count => _clans.Count;

        public IEnumerator<Clan> GetEnumerator()
        {
            return _clans.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }

    public class Clan : IReadOnlyCollection<ClanMember>
    {
        private readonly Dictionary<ulong, ClanMember> _members;
        private ulong _ownerId;

        public ClanMember this[ulong id] => GetMember(id);

        public uint Id { get; }
        public DateTimeOffset CreationDate { get; }
        public string Name { get; }
        public string Icon { get; }
        public string Description { get; }
        public ClubArea Area { get; }
        public ClubActivity Activity { get; }
        public ClubClass Class { get; }
        public ClanMember Owner => GetMember(_ownerId);

        public Clan(ClanEntity entity, IEnumerable<(ClanMemberEntity, AccountEntity)> members)
        {
            Id = (uint)entity.Id;
            CreationDate = DateTimeOffset.FromUnixTimeSeconds(entity.CreationDate);
            Name = entity.Name;
            Icon = entity.Icon;
            Description = entity.Description;
            Area = (ClubArea)entity.Area;
            Activity = (ClubActivity)entity.Activity;
            Class = (ClubClass)entity.Class;

            _members = new Dictionary<ulong, ClanMember>();
            _ownerId = (ulong)entity.OwnerId;

            foreach (var (memberEntity, account) in members)
                _members[(ulong)memberEntity.PlayerId] = new ClanMember(memberEntity, account.Nickname);
        }

        public ClanMember GetMember(ulong id)
        {
            return _members.GetValueOrDefault(id);
        }

        #region IReadOnlyCollection
        public int Count => _members.Count;

        public IEnumerator<ClanMember> GetEnumerator()
        {
            return _members.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }

    public class ClanMember
    {
        private readonly string _cachedName;

        public int Id { get; }
        public DateTimeOffset JoinDate { get; }
        public ClubMemberState State { get; internal set; }
        public ClubRole Role { get; internal set; }
        public ulong AccountId { get; }
        public string Name => Player?.Account.Nickname ?? _cachedName;
        public Player Player { get; internal set; }

        public ClanMember(ClanMemberEntity entity, string name)
        {
            Id = entity.Id;
            JoinDate = DateTimeOffset.FromUnixTimeSeconds(entity.JoinDate);
            State = (ClubMemberState)entity.State;
            Role = (ClubRole)entity.Role;
            AccountId = (ulong)entity.PlayerId;
            _cachedName = name;
        }
    }
}
