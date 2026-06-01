using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DotNetty.Transport.Channels;
using ExpressMapper;
using Foundatio.Caching;
using Foundatio.Messaging;
using Foundatio.Serializer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Netsphere.Common;
using Netsphere.Common.Configuration;
using Netsphere.Common.Plugins;
using Netsphere.Database;
using Netsphere.Database.Game;
using Netsphere.Network.Data.Club;
using Netsphere.Network.Data.Game;
using Netsphere.Network.Data.GameRule;
using Netsphere.Network.Message.Club;
using Netsphere.Network.Message.Game;
using Netsphere.Network.Message.GameRule;
using Netsphere.Network.Serializers;
using Netsphere.Resource.xml;
using Netsphere.Resource.Xml;
using Netsphere.Server.Game.GameRules;
using Netsphere.Server.Game.Serializers;
using Netsphere.Server.Game.Services;
using Newtonsoft.Json;
using ProudNet;
using ProudNet.Hosting;
using ProudNet.Hosting.Services;
using Serilog;
using StackExchange.Redis;

namespace Netsphere.Server.Game
{
    internal static class Program
    {
        public static string BaseDirectory { get; private set; }

        private static void Main()
        {
            BaseDirectory = Environment.GetEnvironmentVariable("NETSPHEREPIRATES_BASEDIR_GAME");
            if (string.IsNullOrWhiteSpace(BaseDirectory))
                BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var configuration = Startup.Initialize(BaseDirectory, "config.hjson",
                x => x.GetSection(nameof(AppOptions.Logging)).Get<LoggerOptions>());

            Log.Information("Starting...");

            var appOptions = configuration.Get<AppOptions>();
            var hostBuilder = new HostBuilder();
            var redisConnectionMultiplexer = ConnectionMultiplexer.Connect(appOptions.Database.ConnectionStrings.Redis);

            IPluginHost pluginHost = new MefPluginHost();
            pluginHost.Initialize(configuration, Path.Combine(BaseDirectory, "plugins"));

            ConfigureMapper();

            hostBuilder
                .ConfigureHostConfiguration(builder => builder.AddConfiguration(configuration))
                .ConfigureAppConfiguration(builder => builder.AddConfiguration(configuration))
                .UseConsoleLifetime()
                .UseProudNetServer(builder =>
                {
                    var messageHandlerResolver = new DefaultMessageHandlerResolver(
                        AppDomain.CurrentDomain.GetAssemblies(),
                        typeof(IGameMessage),
                        typeof(IGameRuleMessage),
                        typeof(IClubMessage)
                    );

                    builder
                        .UseHostIdFactory<HostIdFactory>()
                        .UseSessionFactory<SessionFactory>()
                        .AddMessageFactory<GameMessageFactory>()
                        .AddMessageFactory<GameRuleMessageFactory>()
                        .AddMessageFactory<ClubMessageFactory>()
                        .UseMessageHandlerResolver(messageHandlerResolver)
                        .UseNetworkConfiguration((context, options) =>
                        {
                            options.Version = new Guid("{beb92241-8333-4117-ab92-9b4af78c688f}");
                            options.TcpListener = appOptions.Network.Listener;
                        })
                        .UseThreadingConfiguration((context, options) =>
                        {
                            options.SocketListenerThreadsFactory = () => new MultithreadEventLoopGroup(1);
                            options.SocketWorkerThreadsFactory = () => appOptions.Network.WorkerThreads < 1
                                ? new MultithreadEventLoopGroup()
                                : new MultithreadEventLoopGroup(appOptions.Network.WorkerThreads);
                            options.WorkerThreadFactory = () => new SingleThreadEventLoop();
                        })
                        .ConfigureSerializer(serializer =>
                        {
                            serializer.AddSerializer(new CharacterStyleSerializer());
                            serializer.AddSerializer(new ItemNumberSerializer());
                            serializer.AddSerializer(new VersionSerializer());
                            serializer.AddSerializer(new ShopPriceSerializer());
                            serializer.AddSerializer(new ShopEffectSerializer());
                            serializer.AddSerializer(new ShopItemSerializer());
                            serializer.AddSerializer(new PeerIdSerializer());
                            serializer.AddSerializer(new LongPeerIdSerializer());
                        });
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true)
                        .Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromMinutes(1))
                        .Configure<AppOptions>(context.Configuration)
                        .Configure<NetworkOptions>(context.Configuration.GetSection(nameof(AppOptions.Network)))
                        .Configure<ServerListOptions>(context.Configuration.GetSection(nameof(AppOptions.ServerList)))
                        .Configure<DatabaseOptions>(context.Configuration.GetSection(nameof(AppOptions.Database)))
                        .Configure<GameOptions>(context.Configuration.GetSection(nameof(AppOptions.Game)))
                        .Configure<ClanOptions>(context.Configuration
                            .GetSection(nameof(AppOptions.Game))
                            .GetSection(nameof(AppOptions.Game.ClanOptions)))
                        .Configure<DeathmatchOptions>(context.Configuration
                            .GetSection(nameof(AppOptions.Game))
                            .GetSection(nameof(AppOptions.Game.Deathmatch)))
                        .Configure<TouchdownOptions>(context.Configuration
                            .GetSection(nameof(AppOptions.Game))
                            .GetSection(nameof(AppOptions.Game.Touchdown)))
                        .Configure<BattleRoyalOptions>(context.Configuration
                            .GetSection(nameof(AppOptions.Game))
                            .GetSection(nameof(AppOptions.Game.BattleRoyal)))
                        .Configure<CaptainOptions>(context.Configuration
                            .GetSection(nameof(AppOptions.Game))
                            .GetSection(nameof(AppOptions.Game.Captain)))
                        .Configure<IdGeneratorOptions>(x => x.Id = 0)
                        .AddSingleton<DatabaseService>()
                        .AddDbContext<AuthContext>(x =>
                        {
                            var dbPath = appOptions.Database.ConnectionStrings.Auth;
                            var dir = Path.GetDirectoryName(dbPath);

                            if (!Directory.Exists(dir))
                                Directory.CreateDirectory(dir);
                            x.UseSqlite($"Data Source={dbPath}");
                        })
                        .AddDbContext<GameContext>(x =>
                        {
                            var dbPath = appOptions.Database.ConnectionStrings.Game;
                            var dir = Path.GetDirectoryName(dbPath);

                            if (!Directory.Exists(dir))
                                Directory.CreateDirectory(dir);
                            x.UseSqlite($"Data Source={dbPath}");
                        })
                        .AddSingleton(redisConnectionMultiplexer)
                        .AddTransient<ISerializer>(x => new JsonNetSerializer(JsonConvert.DefaultSettings()))
                        .AddSingleton<ICacheClient, RedisCacheClient>()
                        .AddSingleton<IMessageBus, RedisMessageBus>()
                        .AddSingleton(x => new RedisCacheClientOptions
                        {
                            ConnectionMultiplexer = x.GetRequiredService<ConnectionMultiplexer>(),
                            Serializer = x.GetRequiredService<ISerializer>()
                        })
                        .AddSingleton(x => new RedisMessageBusOptions
                        {
                            Subscriber = x.GetRequiredService<ConnectionMultiplexer>().GetSubscriber(),
                            Serializer = x.GetRequiredService<ISerializer>()
                        })
                        .AddTransient<Player>()
                        .AddTransient<CharacterManager>()
                        .AddTransient<PlayerInventory>()
                        .AddSingleton<PlayerManager>()
                        .AddTransient<RoomManager>()
                        .AddTransient<Room>()
                        .AddSingleton<GameRuleResolver>()
                        .AddTransient<GameRuleStateMachine>()
                        .AddTransient<Deathmatch>()
                        .AddTransient<Touchdown>()
                        .AddTransient<BattleRoyal>()
                        .AddTransient<Captain>()
                        .AddTransient<Practice>()
                        .AddTransient<Captain>()
                        .AddSingleton<EquipValidator>()
                        .AddTransient<Clan>()
                        .AddCommands(typeof(Program).Assembly)
                        .AddService<IdGeneratorService>()
                        .AddService<NicknameLookupService>()
                        .AddHostedServiceEx<ServerlistService>()
                        .AddHostedServiceEx<GameDataService>()
                        .AddHostedServiceEx<ChannelService>()
                        .AddHostedServiceEx<ClanManager>()
                        .AddHostedServiceEx<IpcService>()
                        .AddHostedServiceEx<PlayerSaveService>()
                        .AddHostedServiceEx<CommandService>();

                    pluginHost.OnConfigure(services);
                });

            var host = hostBuilder.Build();

            var contexts = host.Services.GetRequiredService<IEnumerable<DbContext>>();

            foreach (var db in contexts)
            {
                //Disabled log to avoid filling console with too much stuff
                //Log.Information("Ensuring database exists={Context}...", db.GetType().Name);
                db.Database.EnsureCreated();
            }

            var gameDb = host.Services.GetRequiredService<GameContext>();
            InitializeDb(gameDb);

            host.Services
                .GetRequiredService<IProudNetServerService>()
                .UnhandledRmi += (s, e) => Log.Debug("Unhandled Message={@Message} HostId={HostId}", e.Message, e.Session.HostId);

            host.Services.GetRequiredService<IApplicationLifetime>().ApplicationStarted.Register(() =>
                Log.Information("Press Ctrl + C to shutdown"));
            host.Run();
            host.Dispose();
            pluginHost.Dispose();
        }

        private static void InitializeDb(GameContext gameDb)
        {
            if (!gameDb.Channels.Any())
            {
                gameDb.Channels.Add(new ChannelEntity
                {
                    Name = "Default",
                    Description = "Default channel",
                    PlayerLimit = 100,
                    Color = "FFFFFFFF",
                    MinLevel = 0,
                    MaxLevel = 100
                });

                gameDb.SaveChanges();
            }

            if (!gameDb.EffectGroups.Any())
            {
                gameDb.EffectGroups.Add(new ShopEffectGroupEntity
                {
                    Id = 1,
                    Name = "None",
                    PreviewEffect = 0
                });
                gameDb.SaveChanges();
            }

            if (!gameDb.PriceGroups.Any())
            {
                gameDb.PriceGroups.Add(new ShopPriceGroupEntity
                {
                    Id = 1,
                    Name = "Free",
                    PriceType = 2
                });
                gameDb.SaveChanges();
            }

            if (!gameDb.Prices.Any())
            {
                gameDb.Prices.Add(new ShopPriceEntity
                {
                    Id = 1,
                    PriceGroupId = 1,
                    PeriodType = 4,
                    Period = 0,
                    Price = 0,
                    IsRefundable = true,
                    Durability = -1,
                    IsEnabled = true
                });

                gameDb.SaveChanges();
            }

            if (!gameDb.Items.Any())
            {
                var items = new List<Item>();

                var skills = ItemParser.ParseItems("Game/xml/action.x7");
                var weapons = ItemParser.ParseItems("Game/xml/_eu_weapon.x7");
                var clothes = ItemParser.ParseItems("Game/xml/item.x7");

                ProcessItems(gameDb, skills);
                ProcessItems(gameDb, weapons);
                ProcessItems(gameDb, clothes);
            }

            if (!gameDb.StartItems.Any())
            {
                gameDb.StartItems.Add(new StartItemEntity
                {
                    Id = 1,
                    ShopItemInfoId = 25,
                    ShopPriceId = 1,
                    Color = 0,
                    RequiredSecurityLevel = 0
                });
                gameDb.StartItems.Add(new StartItemEntity
                {
                    Id = 2,
                    ShopItemInfoId = 116,
                    ShopPriceId = 1,
                    Color = 0,
                    RequiredSecurityLevel = 0
                });
                gameDb.SaveChanges();
            }
        }

        private static void ProcessItems(GameContext db, List<Item> items)
        {
            foreach (var item in items)
            {
                var id = long.Parse(item.Id);

                if (!ItemLogic.TryGetType(item.Id, out var type))
                    continue;

                var exists = db.Items.Local.FirstOrDefault(x => x.Id == id);
                if (exists != null)
                    continue;

                var tab = ItemLogic.CalculateTabInfo(type);

                db.Items.Add(new ShopItemEntity
                {
                    Id = id,
                    RequiredGender = (byte)item.Gender,
                    RequiredLicense = 10,
                    Colors = 10,
                    UniqueColors = 0,
                    RequiredLevel = 0,
                    LevelLimit = 0,
                    RequiredMasterLevel = 0,
                    IsOneTimeUse = false,
                    IsDestroyable = true,
                    MainTab = (byte)tab.mainTab,
                    SubTab = (byte)tab.subTab
                });
            }

            db.SaveChanges();

            foreach (var item in items)
            {
                var id = long.Parse(item.Id);

                if (!db.Items.Any(x => x.Id == id))
                    continue;

                db.ItemInfos.Add(new ShopItemInfoEntity
                {
                    ShopItemId = id,
                    PriceGroupId = 1,
                    EffectGroupId = 1,
                    DiscountPercentage = 0,
                    IsEnabled = true
                });
            }

            db.SaveChanges();
        }

        private static void ConfigureMapper()
        {
            Mapper.Register<Channel, ChannelInfoDto>()
                .Member(dest => dest.PlayerCount, src => src.Players.Count)
                .Function(dest => dest.IsClanChannel, src => src.Category == ChannelCategory.Club);

            Mapper.Register<PlayerItem, ItemDto>()
                .Member(dest => dest.ExpireTime,
                    src => src.ExpireDate == DateTimeOffset.MinValue ? -1 : src.ExpireDate.ToUnixTimeSeconds())
                .Function(
                    dest => dest.Effects,
                    src => src.Effects.Select(x => new ItemEffectDto
                    {
                        Effect = x
                    }).OrderBy(x => x.Effect).ToArray()
                );

            Mapper.Register<Room, Room2Dto>()
                .Member(dest => dest.RoomId, src => src.Id)
                .Member(dest => dest.GameRule, src => src.Options.GameRule)
                .Member(dest => dest.Map, src => src.Options.Map)
                .Member(dest => dest.PlayerLimit, src => src.Options.PlayerLimit)
                .Member(dest => dest.Name, src => src.Options.Name)
                .Member(dest => dest.ItemLimit, src => src.Options.EquipLimit)
                .Member(dest => dest.PlayerCount, src => src.Players.Count(x => !x.Value.IsInGMMode))
                .Function(dest => dest.State, src => src.GameRule.StateMachine.GameState - 1)
                .Member(dest => dest.IsSpectatingEnabled, src => src.Options.IsSpectatingEnabled)
                .Function(dest => dest.Password, src => string.IsNullOrEmpty(src.Options.Password) ? "" : "***")
                .Function(dest => dest.Settings, src =>
                {
                    var settings = RoomSettings.None;
                    if (src.Options.IsFriendly)
                        settings |= RoomSettings.IsFriendly;

                    return settings;
                });

            Mapper.Register<Room, EnterRoomInfo2Dto>()
                .Member(dest => dest.RoomId, src => src.Id)
                .Member(dest => dest.GameRule, src => src.Options.GameRule)
                .Member(dest => dest.Map, src => src.Options.Map)
                .Member(dest => dest.PlayerLimit, src => src.Options.PlayerLimit)
                .Member(dest => dest.TimeLimit, src => src.Options.TimeLimit.TotalMilliseconds)
                .Member(dest => dest.TimeSync, src => src.GameRule.StateMachine.RoundTime.TotalMilliseconds)
                .Member(dest => dest.ScoreLimit, src => src.Options.ScoreLimit)
                .Member(dest => dest.RelayEndPoint, src => src.Options.RelayEndPoint)
                .Member(dest => dest.State, src => src.GameRule.StateMachine.GameState)
                .Member(dest => dest.TimeState, src => src.GameRule.StateMachine.TimeState);

            Mapper.Register<Player, RoomPlayerDto>()
                .Member(dest => dest.AccountId, src => src.Account.Id)
                .Member(dest => dest.Nickname, src => src.Account.Nickname)
                .Member(dest => dest.Slot, src => src.Slot)
                .Value(dest => dest.Unk2, (byte)144);

            Mapper.Register<RoomCreationOptions, ChangeRuleDto>()
                .Member(dest => dest.GameRule, src => src.GameRule)
                .Member(dest => dest.Map, src => src.Map)
                .Member(dest => dest.PlayerLimit, src => src.PlayerLimit)
                .Member(dest => dest.ScoreLimit, src => src.ScoreLimit)
                .Member(dest => dest.TimeLimit, src => src.TimeLimit)
                .Member(dest => dest.ItemLimit, src => src.EquipLimit)
                .Member(dest => dest.Password, src => src.Password)
                .Member(dest => dest.Name, src => src.Name)
                .Member(dest => dest.IsSpectatingEnabled, src => src.IsSpectatingEnabled)
                .Member(dest => dest.SpectatorLimit, src => src.SpectatorLimit);

            Mapper.Register<RoomCreationOptions, ChangeRule2Dto>()
                .Member(dest => dest.GameRule, src => src.GameRule)
                .Member(dest => dest.Map, src => src.Map)
                .Member(dest => dest.PlayerLimit, src => src.PlayerLimit)
                .Member(dest => dest.ScoreLimit, src => src.ScoreLimit)
                .Member(dest => dest.TimeLimit, src => src.TimeLimit)
                .Member(dest => dest.ItemLimit, src => src.EquipLimit)
                .Member(dest => dest.Password, src => src.Password)
                .Member(dest => dest.Name, src => src.Name)
                .Member(dest => dest.IsSpectatingEnabled, src => src.IsSpectatingEnabled)
                .Member(dest => dest.SpectatorLimit, src => src.SpectatorLimit)
                .Function(dest => dest.Settings, src =>
                {
                    var settings = RoomSettings.None;
                    if (src.IsFriendly)
                        settings |= RoomSettings.IsFriendly;

                    return settings;
                });

            Mapper.Register<Clan, ClubSearchResultDto>()
                .Function(dest => dest.OwnerName, src => src.Owner.Name)
                .Function(dest => dest.MemberCount, src => src.Count);

            Mapper.Register<ClanMember, JoinWaiterInfoDto>();

            Mapper.Compile(CompilationTypes.Source);
        }
    }
}
