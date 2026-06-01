-- MySQL Administrator dump 1.4
--
-- ------------------------------------------------------
-- Server version	5.5.0-m2-community


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO,MYSQL323' */;


--
-- Create schema game
--

--
-- Definition of table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(95) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL,
  PRIMARY KEY (`MigrationId`)
)

--
-- Dumping data for table `__efmigrationshistory`
--

/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
INSERT INTO `__efmigrationshistory` (`MigrationId`,`ProductVersion`) VALUES 
 ('20190118215121_initial','2.1.4-rtm-31024'),
 ('20190121181135_revert_back_to_int32_accountid','2.1.4-rtm-31024'),
 ('20190213174858_add_level_rewards','2.1.4-rtm-31024'),
 ('20190413124246_remove_licenses','2.1.4-rtm-31024'),
 ('20190414105838_add_channels','2.1.4-rtm-31024'),
 ('20190414112534_htmlhex_color','2.1.4-rtm-31024'),
 ('20190429114839_update_shop','2.1.4-rtm-31024'),
 ('20190429125636_add_mp_to_items','2.1.4-rtm-31024'),
 ('20190429130959_remove_item_count','2.1.4-rtm-31024'),
 ('20190506095304_update_multiple_item_effects','2.1.4-rtm-31024'),
 ('20190506133559_remove_startitem_shopeffect_column','2.1.4-rtm-31024'),
 ('20190512122515_add_petId_column','2.1.4-rtm-31024'),
 ('20190618082852_add_effect_preview','2.1.4-rtm-31024'),
 ('20190709145056_add_friends','2.1.4-rtm-31024'),
 ('20190709154614_remove_','2.1.4-rtm-31024'),
 ('20190805175509_add_clans','2.1.4-rtm-31024'),
 ('20190819085520_add_clanmember_lastlogindate','2.1.4-rtm-31024'),
 ('20190819100155_add_clan_join_conditions','2.1.4-rtm-31024'),
 ('20190819103352_add_clanmember_join_answers','2.1.4-rtm-31024'),
 ('20190820081106_add_clan_bans','2.1.4-rtm-31024'),
 ('20190820120623_add_clan_events','2.1.4-rtm-31024');
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;


--
-- Definition of table `channels`
--

DROP TABLE IF EXISTS `channels`;
CREATE TABLE `channels` (
  `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
  `PlayerLimit` int(11) NOT NULL,
  `Name` varchar(40) NOT NULL,
  `Description` varchar(40) NOT NULL,
  `Color` varchar(8) NOT NULL,
  `MinLevel` int(11) NOT NULL,
  `MaxLevel` int(11) NOT NULL
)

--
-- Dumping data for table `channels`
--

/*!40000 ALTER TABLE `channels` DISABLE KEYS */;
/*!40000 ALTER TABLE `channels` ENABLE KEYS */;


--
-- Definition of table `clan_bans`
--

DROP TABLE IF EXISTS `clan_bans`;
CREATE TABLE `clan_bans` (
  `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
  `ClanId` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `Date` bigint(20) NOT NULL,
  FOREIGN KEY (ClanId) REFERENCES clans(Id),
  FOREIGN KEY (PlayerId) REFERENCES players(Id)
);

CREATE INDEX IX_clan_bans_ClanId ON clan_bans(ClanId);
CREATE INDEX IX_clan_bans_PlayerId ON clan_bans(PlayerId);

--
-- Dumping data for table `clan_bans`
--

/*!40000 ALTER TABLE `clan_bans` DISABLE KEYS */;
/*!40000 ALTER TABLE `clan_bans` ENABLE KEYS */;


--
-- Definition of table `clan_events`
--

DROP TABLE IF EXISTS `clan_events`;
CREATE TABLE `clan_events` (
  `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
  `ClanId` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `Date` bigint(20) NOT NULL,
  `Type` tinyint(3) NOT NULL,
  `Value1` bigint(20) NOT NULL,
  FOREIGN KEY (ClanId) REFERENCES clans(Id),
  FOREIGN KEY (PlayerId) REFERENCES players(Id)
);

CREATE INDEX IX_clan_events_ClanId ON clan_events(ClanId);
CREATE INDEX IX_clan_events_PlayerId ON clan_events(PlayerId);

--
-- Dumping data for table `clan_events`
--

/*!40000 ALTER TABLE `clan_events` DISABLE KEYS */;
/*!40000 ALTER TABLE `clan_events` ENABLE KEYS */;


--
-- Definition of table `clan_members`
--

DROP TABLE IF EXISTS `clan_members`;
CREATE TABLE `clan_members` (
  `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
  `ClanId` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `JoinDate` bigint(20) NOT NULL,
  `State` tinyint(3) NOT NULL,
  `Role` tinyint(3) NOT NULL,
  `LastLoginDate` bigint(20) NOT NULL DEFAULT '0',
  `Answer1` longtext,
  `Answer2` longtext,
  `Answer3` longtext,
  `Answer4` longtext,
  `Answer5` longtext,
  FOREIGN KEY (PlayerId) REFERENCES clans(Id),
  FOREIGN KEY (ClanId) REFERENCES clans(Id)
);

CREATE INDEX IX_clan_members_clans_ClanId ON clan_members(ClanId);
CREATE INDEX IX_clan_members_players_PlayerId ON clan_members(PlayerId);

--
-- Dumping data for table `clan_members`
--

/*!40000 ALTER TABLE `clan_members` DISABLE KEYS */;
/*!40000 ALTER TABLE `clan_members` ENABLE KEYS */;


--
-- Definition of table `clans`
--
DROP TABLE IF EXISTS clans;
CREATE TABLE clans (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  OwnerId INTEGER NOT NULL,
  CreationDate BIGINT NOT NULL,
  Name VARCHAR(40) NOT NULL,
  Icon VARCHAR(8) NOT NULL,
  Description VARCHAR(40) DEFAULT NULL,
  Area TINYINT NOT NULL,
  Activity TINYINT NOT NULL,
  Question1 VARCHAR(40) DEFAULT NULL,
  Question2 VARCHAR(40) DEFAULT NULL,
  Question3 VARCHAR(40) DEFAULT NULL,
  Question4 VARCHAR(40) DEFAULT NULL,
  Question5 VARCHAR(40) DEFAULT NULL,
  Class TINYINT NOT NULL,
  Announcement VARCHAR(40) DEFAULT NULL,
  IsPublic INTEGER NOT NULL DEFAULT 0,
  RequiredLevel TINYINT NOT NULL DEFAULT 0,
  CONSTRAINT IX_clans_Name UNIQUE (Name),
  CONSTRAINT FK_clans_players_OwnerId
    FOREIGN KEY (OwnerId)
    REFERENCES players (Id)
    ON DELETE CASCADE
);

CREATE INDEX IX_clans_OwnerId
ON clans (OwnerId);

--
-- Dumping data for table `clans`
--

/*!40000 ALTER TABLE `clans` DISABLE KEYS */;
/*!40000 ALTER TABLE `clans` ENABLE KEYS */;


--
-- Definition of table `level_rewards`
--

DROP TABLE IF EXISTS `level_rewards`;
CREATE TABLE `level_rewards` (
  `Level` int(11) NOT NULL,
  `MoneyType` tinyint(3) NOT NULL,
  `Money` int(11) NOT NULL,
  PRIMARY KEY (`Level`)
)

--
-- Dumping data for table `level_rewards`
--

/*!40000 ALTER TABLE `level_rewards` DISABLE KEYS */;
/*!40000 ALTER TABLE `level_rewards` ENABLE KEYS */;


--
-- Definition of table `player_characters`
--

DROP TABLE IF EXISTS player_characters;

CREATE TABLE player_characters (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  PlayerId INTEGER NOT NULL,
  Slot TINYINT NOT NULL,
  Gender TINYINT NOT NULL,
  BasicHair TINYINT NOT NULL,
  BasicFace TINYINT NOT NULL,
  BasicShirt TINYINT NOT NULL,
  BasicPants TINYINT NOT NULL,
  Weapon1Id BIGINT DEFAULT NULL,
  Weapon2Id BIGINT DEFAULT NULL,
  Weapon3Id BIGINT DEFAULT NULL,
  SkillId BIGINT DEFAULT NULL,
  HairId BIGINT DEFAULT NULL,
  FaceId BIGINT DEFAULT NULL,
  ShirtId BIGINT DEFAULT NULL,
  PantsId BIGINT DEFAULT NULL,
  GlovesId BIGINT DEFAULT NULL,
  ShoesId BIGINT DEFAULT NULL,
  AccessoryId BIGINT DEFAULT NULL,
  PetId BIGINT DEFAULT NULL,


  CONSTRAINT FK_player_characters_player_items_PetId
    FOREIGN KEY (PetId)
    REFERENCES player_items (Id),

  CONSTRAINT FK_player_characters_players_PlayerId
    FOREIGN KEY (PlayerId)
    REFERENCES players (Id)
    ON DELETE CASCADE,

  CONSTRAINT FK_player_characters_player_items_AccessoryId
    FOREIGN KEY (AccessoryId)
    REFERENCES player_items (Id),

  CONSTRAINT FK_player_characters_player_items_FaceId
    FOREIGN KEY (FaceId)
    REFERENCES player_items (Id),

  CONSTRAINT FK_player_characters_player_items_GlovesId
    FOREIGN KEY (GlovesId)
    REFERENCES player_items (Id),

  CONSTRAINT FK_player_characters_player_items_HairId
    FOREIGN KEY (HairId)
    REFERENCES player_items (Id),

  CONSTRAINT FK_player_characters_player_items_PantsId
    FOREIGN KEY (PantsId)
    REFERENCES player_items (Id),

  CONSTRAINT FK_player_characters_player_items_ShirtId
    FOREIGN KEY (ShirtId)
    REFERENCES player_items (Id),

  CONSTRAINT FK_player_characters_player_items_ShoesId
    FOREIGN KEY (ShoesId)
    REFERENCES player_items (Id),

  CONSTRAINT FK_player_characters_player_items_SkillId
    FOREIGN KEY (SkillId)
    REFERENCES player_items (Id),

  CONSTRAINT FK_player_characters_player_items_Weapon1Id
    FOREIGN KEY (Weapon1Id)
    REFERENCES player_items (Id),

  CONSTRAINT FK_player_characters_player_items_Weapon2Id
    FOREIGN KEY (Weapon2Id)
    REFERENCES player_items (Id),

  CONSTRAINT FK_player_characters_player_items_Weapon3Id
    FOREIGN KEY (Weapon3Id)
    REFERENCES player_items (Id)
);

CREATE INDEX IX_player_characters_AccessoryId
ON player_characters (AccessoryId);

CREATE INDEX IX_player_characters_FaceId
ON player_characters (FaceId);

CREATE INDEX IX_player_characters_GlovesId
ON player_characters (GlovesId);

CREATE INDEX IX_player_characters_HairId
ON player_characters (HairId);

CREATE INDEX IX_player_characters_PantsId
ON player_characters (PantsId);

CREATE INDEX IX_player_characters_PlayerId
ON player_characters (PlayerId);

CREATE INDEX IX_player_characters_ShirtId
ON player_characters (ShirtId);

CREATE INDEX IX_player_characters_ShoesId
ON player_characters (ShoesId);

CREATE INDEX IX_player_characters_SkillId
ON player_characters (SkillId);

CREATE INDEX IX_player_characters_Weapon1Id
ON player_characters (Weapon1Id);

CREATE INDEX IX_player_characters_Weapon2Id
ON player_characters (Weapon2Id);

CREATE INDEX IX_player_characters_Weapon3Id
ON player_characters (Weapon3Id);

CREATE INDEX IX_player_characters_PetId
ON player_characters (PetId);

--
-- Dumping data for table `player_characters`
--

/*!40000 ALTER TABLE `player_characters` DISABLE KEYS */;
/*!40000 ALTER TABLE `player_characters` ENABLE KEYS */;


--
-- Definition of table `player_deny`
--

DROP TABLE IF EXISTS player_deny;

CREATE TABLE player_deny (
  Id BIGINT NOT NULL,
  PlayerId INTEGER NOT NULL,
  DenyPlayerId INTEGER NOT NULL,

  PRIMARY KEY (Id),

  CONSTRAINT FK_player_deny_players_DenyPlayerId
    FOREIGN KEY (DenyPlayerId)
    REFERENCES players (Id)
    ON DELETE CASCADE,

  CONSTRAINT FK_player_deny_players_PlayerId
    FOREIGN KEY (PlayerId)
    REFERENCES players (Id)
    ON DELETE CASCADE
);

CREATE INDEX IX_player_deny_DenyPlayerId
ON player_deny (DenyPlayerId);

CREATE INDEX IX_player_deny_PlayerId
ON player_deny (PlayerId);

--
-- Dumping data for table `player_deny`
--

/*!40000 ALTER TABLE `player_deny` DISABLE KEYS */;
/*!40000 ALTER TABLE `player_deny` ENABLE KEYS */;


--
-- Definition of table `player_friends`
--

DROP TABLE IF EXISTS player_friends;

CREATE TABLE player_friends (
  Id BIGINT NOT NULL,
  PlayerId INTEGER NOT NULL,
  FriendPlayerId INTEGER NOT NULL,
  State TINYINT NOT NULL,

  PRIMARY KEY (Id),

  CONSTRAINT FK_player_friends_players_FriendPlayerId
    FOREIGN KEY (FriendPlayerId)
    REFERENCES players (Id)
    ON DELETE CASCADE,

  CONSTRAINT FK_player_friends_players_PlayerId
    FOREIGN KEY (PlayerId)
    REFERENCES players (Id)
    ON DELETE CASCADE
);

CREATE INDEX IX_player_friends_FriendPlayerId
ON player_friends (FriendPlayerId);

CREATE INDEX IX_player_friends_PlayerId
ON player_friends (PlayerId);

--
-- Dumping data for table `player_friends`
--

/*!40000 ALTER TABLE `player_friends` DISABLE KEYS */;
/*!40000 ALTER TABLE `player_friends` ENABLE KEYS */;


--
-- Definition of table `player_items`
--

DROP TABLE IF EXISTS player_items;

CREATE TABLE player_items (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  PlayerId INTEGER NOT NULL,
  ShopItemInfoId INTEGER NOT NULL,
  ShopPriceId INTEGER NOT NULL,
  Color TINYINT NOT NULL,
  PurchaseDate BIGINT NOT NULL,
  Durability INTEGER NOT NULL,
  MP INTEGER NOT NULL DEFAULT 0,
  MPLevel INTEGER NOT NULL DEFAULT 0,
  Effects TEXT DEFAULT NULL,

  CONSTRAINT FK_player_items_players_PlayerId
    FOREIGN KEY (PlayerId)
    REFERENCES players (Id)
    ON DELETE CASCADE,

  CONSTRAINT FK_player_items_shop_iteminfos_ShopItemInfoId
    FOREIGN KEY (ShopItemInfoId)
    REFERENCES shop_iteminfos (Id)
    ON DELETE CASCADE,

  CONSTRAINT FK_player_items_shop_prices_ShopPriceId
    FOREIGN KEY (ShopPriceId)
    REFERENCES shop_prices (Id)
    ON DELETE CASCADE
);

CREATE INDEX IX_player_items_PlayerId
ON player_items (PlayerId);

CREATE INDEX IX_player_items_ShopItemInfoId
ON player_items (ShopItemInfoId);

CREATE INDEX IX_player_items_ShopPriceId
ON player_items (ShopPriceId);

--
-- Dumping data for table `player_items`
--

/*!40000 ALTER TABLE `player_items` DISABLE KEYS */;
/*!40000 ALTER TABLE `player_items` ENABLE KEYS */;


--
-- Definition of table `player_mails`
--

DROP TABLE IF EXISTS player_mails;

CREATE TABLE player_mails (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  PlayerId INTEGER NOT NULL,
  SenderPlayerId INTEGER NOT NULL,
  SentDate BIGINT NOT NULL,
  Title VARCHAR(100) NOT NULL,
  Message VARCHAR(500) NOT NULL,
  IsMailNew INTEGER NOT NULL,
  IsMailDeleted INTEGER NOT NULL,


  CONSTRAINT FK_player_mails_players_PlayerId
    FOREIGN KEY (PlayerId)
    REFERENCES players (Id)
    ON DELETE CASCADE,

  CONSTRAINT FK_player_mails_players_SenderPlayerId
    FOREIGN KEY (SenderPlayerId)
    REFERENCES players (Id)
    ON DELETE CASCADE
);

CREATE INDEX IX_player_mails_PlayerId
ON player_mails (PlayerId);

CREATE INDEX IX_player_mails_SenderPlayerId
ON player_mails (SenderPlayerId);

--
-- Dumping data for table `player_mails`
--

/*!40000 ALTER TABLE `player_mails` DISABLE KEYS */;
/*!40000 ALTER TABLE `player_mails` ENABLE KEYS */;


--
-- Definition of table `player_settings`
--
DROP TABLE IF EXISTS player_settings;

CREATE TABLE player_settings (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  PlayerId INTEGER NOT NULL,
  Setting VARCHAR(100) NOT NULL,
  Value VARCHAR(512) NOT NULL,


  CONSTRAINT FK_player_settings_players_PlayerId
    FOREIGN KEY (PlayerId)
    REFERENCES players (Id)
    ON DELETE CASCADE
);

CREATE INDEX IX_player_settings_PlayerId
ON player_settings (PlayerId);




--
-- Definition of table `players`
--

DROP TABLE IF EXISTS players;

CREATE TABLE players (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  TutorialState INTEGER NOT NULL,
  TotalExperience INTEGER NOT NULL,
  PEN INTEGER NOT NULL,
  AP INTEGER NOT NULL,
  Coins1 INTEGER NOT NULL,
  Coins2 INTEGER NOT NULL,
  CurrentCharacterSlot INTEGER NOT NULL
);
--
-- Dumping data for table `players`
--

/*!40000 ALTER TABLE `players` DISABLE KEYS */;
/*!40000 ALTER TABLE `players` ENABLE KEYS */;





--
-- Definition of table `shop_iteminfos`
--

DROP TABLE IF EXISTS shop_iteminfos;

CREATE TABLE shop_iteminfos (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  ShopItemId BIGINT NOT NULL,
  PriceGroupId INTEGER NOT NULL,
  EffectGroupId INTEGER NOT NULL,
  DiscountPercentage TINYINT NOT NULL,
  IsEnabled INTEGER NOT NULL,


  CONSTRAINT FK_shop_iteminfos_shop_effect_groups_EffectGroupId
    FOREIGN KEY (EffectGroupId)
    REFERENCES shop_effect_groups (Id)
    ON DELETE CASCADE,

  CONSTRAINT FK_shop_iteminfos_shop_price_groups_PriceGroupId
    FOREIGN KEY (PriceGroupId)
    REFERENCES shop_price_groups (Id)
    ON DELETE CASCADE,

  CONSTRAINT FK_shop_iteminfos_shop_items_ShopItemId
    FOREIGN KEY (ShopItemId)
    REFERENCES shop_items (Id)
    ON DELETE CASCADE
);

CREATE INDEX IX_shop_iteminfos_EffectGroupId
ON shop_iteminfos (EffectGroupId);

CREATE INDEX IX_shop_iteminfos_PriceGroupId
ON shop_iteminfos (PriceGroupId);

CREATE INDEX IX_shop_iteminfos_ShopItemId
ON shop_iteminfos (ShopItemId);

--
-- Dumping data for table `shop_iteminfos`
--

/*!40000 ALTER TABLE `shop_iteminfos` DISABLE KEYS */;
/*!40000 ALTER TABLE `shop_iteminfos` ENABLE KEYS */;







--
-- Definition of table `shop_prices`
--

DROP TABLE IF EXISTS shop_prices;

CREATE TABLE shop_prices (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  PriceGroupId INTEGER NOT NULL,
  PeriodType TINYINT NOT NULL,
  Period INTEGER NOT NULL,
  Price INTEGER NOT NULL,
  IsRefundable INTEGER NOT NULL,
  Durability INTEGER NOT NULL,
  IsEnabled INTEGER NOT NULL,


  CONSTRAINT FK_shop_prices_shop_price_groups_PriceGroupId
    FOREIGN KEY (PriceGroupId)
    REFERENCES shop_price_groups (Id)
    ON DELETE CASCADE
);

CREATE INDEX IX_shop_prices_PriceGroupId
ON shop_prices (PriceGroupId);





--
-- Definition of table `shop_price_groups`
--
DROP TABLE IF EXISTS shop_price_groups;

CREATE TABLE shop_price_groups (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Name VARCHAR(20) NOT NULL,
  PriceType TINYINT UNSIGNED NOT NULL,

  CONSTRAINT IX_shop_price_groups_Name UNIQUE (Name)
);

--
-- Dumping data for table `shop_price_groups`
--

/*!40000 ALTER TABLE `shop_price_groups` DISABLE KEYS */;
/*!40000 ALTER TABLE `shop_price_groups` ENABLE KEYS */;

--
-- Definition of table `shop_items`
--
DROP TABLE IF EXISTS shop_items;

CREATE TABLE shop_items (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  RequiredGender TINYINT UNSIGNED NOT NULL,
  RequiredLicense TINYINT UNSIGNED NOT NULL,
  Colors TINYINT UNSIGNED NOT NULL,
  UniqueColors TINYINT UNSIGNED NOT NULL,
  RequiredLevel TINYINT UNSIGNED NOT NULL,
  LevelLimit TINYINT UNSIGNED NOT NULL,
  RequiredMasterLevel TINYINT UNSIGNED NOT NULL,
  IsOneTimeUse INTEGER NOT NULL,
  IsDestroyable INTEGER NOT NULL,
  MainTab TINYINT UNSIGNED NOT NULL DEFAULT 0,
  SubTab TINYINT UNSIGNED NOT NULL DEFAULT 0
);

--
-- Dumping data for table `shop_items`
--

/*!40000 ALTER TABLE `shop_items` DISABLE KEYS */;
/*!40000 ALTER TABLE `shop_items` ENABLE KEYS */;




DROP TABLE IF EXISTS shop_effect_groups;
CREATE TABLE shop_effect_groups (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Name VARCHAR(20) NOT NULL,
  PreviewEffect INTEGER NOT NULL DEFAULT 0,

  CONSTRAINT IX_shop_effect_groups_Name UNIQUE (Name)
);

--
-- Dumping data for table `shop_effect_groups`
--

/*!40000 ALTER TABLE `shop_effect_groups` DISABLE KEYS */;
/*!40000 ALTER TABLE `shop_effect_groups` ENABLE KEYS */;


--
-- Definition of table `shop_effects`
--

DROP TABLE IF EXISTS shop_effects;

CREATE TABLE shop_effects (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  EffectGroupId INTEGER NOT NULL,
  Effect INTEGER NOT NULL,

  CONSTRAINT FK_shop_effects_shop_effect_groups_EffectGroupId
    FOREIGN KEY (EffectGroupId)
    REFERENCES shop_effect_groups (Id)
    ON DELETE CASCADE
);

CREATE INDEX IX_shop_effects_EffectGroupId
ON shop_effects (EffectGroupId);

--
-- Dumping data for table `shop_effects`
--

/*!40000 ALTER TABLE `shop_effects` DISABLE KEYS */;
/*!40000 ALTER TABLE `shop_effects` ENABLE KEYS */;






--
-- Definition of table `shop_version`
--
DROP TABLE IF EXISTS shop_version;

CREATE TABLE shop_version (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Version VARCHAR(40) NOT NULL
);

--
-- Dumping data for table `shop_version`
--

/*!40000 ALTER TABLE `shop_version` DISABLE KEYS */;
/*!40000 ALTER TABLE `shop_version` ENABLE KEYS */;


--
-- Definition of table `start_items`
--

DROP TABLE IF EXISTS start_items;

CREATE TABLE start_items (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  ShopItemInfoId INTEGER NOT NULL,
  ShopPriceId INTEGER NOT NULL,
  Color TINYINT UNSIGNED NOT NULL,
  RequiredSecurityLevel TINYINT UNSIGNED NOT NULL,

  CONSTRAINT FK_start_items_shop_iteminfos_ShopItemInfoId
    FOREIGN KEY (ShopItemInfoId)
    REFERENCES shop_iteminfos (Id)
    ON DELETE CASCADE,

  CONSTRAINT FK_start_items_shop_prices_ShopPriceId
    FOREIGN KEY (ShopPriceId)
    REFERENCES shop_prices (Id)
    ON DELETE CASCADE
);

CREATE INDEX IX_start_items_ShopItemInfoId
ON start_items (ShopItemInfoId);

CREATE INDEX IX_start_items_ShopPriceId
ON start_items (ShopPriceId);

--
-- Dumping data for table `start_items`
--

/*!40000 ALTER TABLE `start_items` DISABLE KEYS */;
/*!40000 ALTER TABLE `start_items` ENABLE KEYS */;




/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;







