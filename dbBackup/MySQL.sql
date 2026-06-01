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
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;


--
-- Create schema auth
--

CREATE DATABASE IF NOT EXISTS auth;
USE auth;

--
-- Definition of table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(95) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `__efmigrationshistory`
--

/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
INSERT INTO `__efmigrationshistory` (`MigrationId`,`ProductVersion`) VALUES 
 ('20190118200022_initial','2.1.4-rtm-31024'),
 ('20190121180916_revert_back_to_int32_accountid','2.1.4-rtm-31024');
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;


--
-- Definition of table `accounts`
--

DROP TABLE IF EXISTS `accounts`;
CREATE TABLE `accounts` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Username` varchar(40) NOT NULL,
  `Nickname` varchar(40) DEFAULT NULL,
  `Password` varchar(40) DEFAULT NULL,
  `Salt` varchar(40) DEFAULT NULL,
  `SecurityLevel` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_accounts_Username` (`Username`),
  UNIQUE KEY `IX_accounts_Nickname` (`Nickname`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `accounts`
--

/*!40000 ALTER TABLE `accounts` DISABLE KEYS */;
/*!40000 ALTER TABLE `accounts` ENABLE KEYS */;


--
-- Definition of table `bans`
--

DROP TABLE IF EXISTS `bans`;
CREATE TABLE `bans` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AccountId` int(11) NOT NULL,
  `Date` bigint(20) NOT NULL,
  `Duration` bigint(20) DEFAULT NULL,
  `Reason` longtext,
  PRIMARY KEY (`Id`),
  KEY `IX_bans_AccountId` (`AccountId`),
  CONSTRAINT `FK_bans_accounts_AccountId` FOREIGN KEY (`AccountId`) REFERENCES `accounts` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `bans`
--

/*!40000 ALTER TABLE `bans` DISABLE KEYS */;
/*!40000 ALTER TABLE `bans` ENABLE KEYS */;


--
-- Definition of table `login_history`
--

DROP TABLE IF EXISTS `login_history`;
CREATE TABLE `login_history` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AccountId` int(11) NOT NULL,
  `Date` bigint(20) NOT NULL,
  `IP` varchar(15) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_login_history_AccountId` (`AccountId`),
  CONSTRAINT `FK_login_history_accounts_AccountId` FOREIGN KEY (`AccountId`) REFERENCES `accounts` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `login_history`
--

/*!40000 ALTER TABLE `login_history` DISABLE KEYS */;
/*!40000 ALTER TABLE `login_history` ENABLE KEYS */;


--
-- Definition of table `nickname_history`
--

DROP TABLE IF EXISTS `nickname_history`;
CREATE TABLE `nickname_history` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AccountId` int(11) NOT NULL,
  `Nickname` varchar(40) NOT NULL,
  `ExpireDate` bigint(20) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_nickname_history_AccountId` (`AccountId`),
  CONSTRAINT `FK_nickname_history_accounts_AccountId` FOREIGN KEY (`AccountId`) REFERENCES `accounts` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `nickname_history`
--

/*!40000 ALTER TABLE `nickname_history` DISABLE KEYS */;
/*!40000 ALTER TABLE `nickname_history` ENABLE KEYS */;

--
-- Create schema game
--

CREATE DATABASE IF NOT EXISTS game;
USE game;

--
-- Definition of table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(95) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

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
 ('20190709154614_remove_auto_increment','2.1.4-rtm-31024'),
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
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `PlayerLimit` int(11) NOT NULL,
  `Name` varchar(40) NOT NULL,
  `Description` varchar(40) NOT NULL,
  `Color` varchar(8) NOT NULL,
  `MinLevel` int(11) NOT NULL,
  `MaxLevel` int(11) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

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
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ClanId` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `Date` bigint(20) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_clan_bans_ClanId` (`ClanId`),
  KEY `IX_clan_bans_PlayerId` (`PlayerId`),
  CONSTRAINT `FK_clan_bans_clans_ClanId` FOREIGN KEY (`ClanId`) REFERENCES `clans` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_clan_bans_players_PlayerId` FOREIGN KEY (`PlayerId`) REFERENCES `players` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

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
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ClanId` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `Date` bigint(20) NOT NULL,
  `Type` tinyint(3) unsigned NOT NULL,
  `Value1` bigint(20) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_clan_events_ClanId` (`ClanId`),
  KEY `IX_clan_events_PlayerId` (`PlayerId`),
  CONSTRAINT `FK_clan_events_clans_ClanId` FOREIGN KEY (`ClanId`) REFERENCES `clans` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_clan_events_players_PlayerId` FOREIGN KEY (`PlayerId`) REFERENCES `players` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

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
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ClanId` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `JoinDate` bigint(20) NOT NULL,
  `State` tinyint(3) unsigned NOT NULL,
  `Role` tinyint(3) unsigned NOT NULL,
  `LastLoginDate` bigint(20) NOT NULL DEFAULT '0',
  `Answer1` longtext,
  `Answer2` longtext,
  `Answer3` longtext,
  `Answer4` longtext,
  `Answer5` longtext,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_clan_members_PlayerId` (`PlayerId`),
  KEY `IX_clan_members_ClanId` (`ClanId`),
  CONSTRAINT `FK_clan_members_clans_ClanId` FOREIGN KEY (`ClanId`) REFERENCES `clans` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_clan_members_players_PlayerId` FOREIGN KEY (`PlayerId`) REFERENCES `players` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `clan_members`
--

/*!40000 ALTER TABLE `clan_members` DISABLE KEYS */;
/*!40000 ALTER TABLE `clan_members` ENABLE KEYS */;


--
-- Definition of table `clans`
--

DROP TABLE IF EXISTS `clans`;
CREATE TABLE `clans` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `OwnerId` int(11) NOT NULL,
  `CreationDate` bigint(20) NOT NULL,
  `Name` varchar(40) NOT NULL,
  `Icon` varchar(8) NOT NULL,
  `Description` varchar(40) DEFAULT NULL,
  `Area` tinyint(3) unsigned NOT NULL,
  `Activity` tinyint(3) unsigned NOT NULL,
  `Question1` varchar(40) DEFAULT NULL,
  `Question2` varchar(40) DEFAULT NULL,
  `Question3` varchar(40) DEFAULT NULL,
  `Question4` varchar(40) DEFAULT NULL,
  `Question5` varchar(40) DEFAULT NULL,
  `Class` tinyint(3) unsigned NOT NULL,
  `Announcement` varchar(40) DEFAULT NULL,
  `IsPublic` bit(1) NOT NULL DEFAULT b'0',
  `RequiredLevel` tinyint(3) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_clans_Name` (`Name`),
  KEY `IX_clans_OwnerId` (`OwnerId`),
  CONSTRAINT `FK_clans_players_OwnerId` FOREIGN KEY (`OwnerId`) REFERENCES `players` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

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
  `MoneyType` tinyint(3) unsigned NOT NULL,
  `Money` int(11) NOT NULL,
  PRIMARY KEY (`Level`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `level_rewards`
--

/*!40000 ALTER TABLE `level_rewards` DISABLE KEYS */;
/*!40000 ALTER TABLE `level_rewards` ENABLE KEYS */;


--
-- Definition of table `player_characters`
--

DROP TABLE IF EXISTS `player_characters`;
CREATE TABLE `player_characters` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `PlayerId` int(11) NOT NULL,
  `Slot` tinyint(3) unsigned NOT NULL,
  `Gender` tinyint(3) unsigned NOT NULL,
  `BasicHair` tinyint(3) unsigned NOT NULL,
  `BasicFace` tinyint(3) unsigned NOT NULL,
  `BasicShirt` tinyint(3) unsigned NOT NULL,
  `BasicPants` tinyint(3) unsigned NOT NULL,
  `Weapon1Id` bigint(20) DEFAULT NULL,
  `Weapon2Id` bigint(20) DEFAULT NULL,
  `Weapon3Id` bigint(20) DEFAULT NULL,
  `SkillId` bigint(20) DEFAULT NULL,
  `HairId` bigint(20) DEFAULT NULL,
  `FaceId` bigint(20) DEFAULT NULL,
  `ShirtId` bigint(20) DEFAULT NULL,
  `PantsId` bigint(20) DEFAULT NULL,
  `GlovesId` bigint(20) DEFAULT NULL,
  `ShoesId` bigint(20) DEFAULT NULL,
  `AccessoryId` bigint(20) DEFAULT NULL,
  `PetId` bigint(20) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_player_characters_AccessoryId` (`AccessoryId`),
  KEY `IX_player_characters_FaceId` (`FaceId`),
  KEY `IX_player_characters_GlovesId` (`GlovesId`),
  KEY `IX_player_characters_HairId` (`HairId`),
  KEY `IX_player_characters_PantsId` (`PantsId`),
  KEY `IX_player_characters_PlayerId` (`PlayerId`),
  KEY `IX_player_characters_ShirtId` (`ShirtId`),
  KEY `IX_player_characters_ShoesId` (`ShoesId`),
  KEY `IX_player_characters_SkillId` (`SkillId`),
  KEY `IX_player_characters_Weapon1Id` (`Weapon1Id`),
  KEY `IX_player_characters_Weapon2Id` (`Weapon2Id`),
  KEY `IX_player_characters_Weapon3Id` (`Weapon3Id`),
  KEY `IX_player_characters_PetId` (`PetId`),
  CONSTRAINT `FK_player_characters_player_items_PetId` FOREIGN KEY (`PetId`) REFERENCES `player_items` (`Id`),
  CONSTRAINT `FK_player_characters_players_PlayerId` FOREIGN KEY (`PlayerId`) REFERENCES `players` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_player_characters_player_items_AccessoryId` FOREIGN KEY (`AccessoryId`) REFERENCES `player_items` (`Id`),
  CONSTRAINT `FK_player_characters_player_items_FaceId` FOREIGN KEY (`FaceId`) REFERENCES `player_items` (`Id`),
  CONSTRAINT `FK_player_characters_player_items_GlovesId` FOREIGN KEY (`GlovesId`) REFERENCES `player_items` (`Id`),
  CONSTRAINT `FK_player_characters_player_items_HairId` FOREIGN KEY (`HairId`) REFERENCES `player_items` (`Id`),
  CONSTRAINT `FK_player_characters_player_items_PantsId` FOREIGN KEY (`PantsId`) REFERENCES `player_items` (`Id`),
  CONSTRAINT `FK_player_characters_player_items_ShirtId` FOREIGN KEY (`ShirtId`) REFERENCES `player_items` (`Id`),
  CONSTRAINT `FK_player_characters_player_items_ShoesId` FOREIGN KEY (`ShoesId`) REFERENCES `player_items` (`Id`),
  CONSTRAINT `FK_player_characters_player_items_SkillId` FOREIGN KEY (`SkillId`) REFERENCES `player_items` (`Id`),
  CONSTRAINT `FK_player_characters_player_items_Weapon1Id` FOREIGN KEY (`Weapon1Id`) REFERENCES `player_items` (`Id`),
  CONSTRAINT `FK_player_characters_player_items_Weapon2Id` FOREIGN KEY (`Weapon2Id`) REFERENCES `player_items` (`Id`),
  CONSTRAINT `FK_player_characters_player_items_Weapon3Id` FOREIGN KEY (`Weapon3Id`) REFERENCES `player_items` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `player_characters`
--

/*!40000 ALTER TABLE `player_characters` DISABLE KEYS */;
/*!40000 ALTER TABLE `player_characters` ENABLE KEYS */;


--
-- Definition of table `player_deny`
--

DROP TABLE IF EXISTS `player_deny`;
CREATE TABLE `player_deny` (
  `Id` bigint(20) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `DenyPlayerId` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_player_deny_DenyPlayerId` (`DenyPlayerId`),
  KEY `IX_player_deny_PlayerId` (`PlayerId`),
  CONSTRAINT `FK_player_deny_players_DenyPlayerId` FOREIGN KEY (`DenyPlayerId`) REFERENCES `players` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_player_deny_players_PlayerId` FOREIGN KEY (`PlayerId`) REFERENCES `players` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `player_deny`
--

/*!40000 ALTER TABLE `player_deny` DISABLE KEYS */;
/*!40000 ALTER TABLE `player_deny` ENABLE KEYS */;


--
-- Definition of table `player_friends`
--

DROP TABLE IF EXISTS `player_friends`;
CREATE TABLE `player_friends` (
  `Id` bigint(20) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `FriendPlayerId` int(11) NOT NULL,
  `State` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_player_friends_FriendPlayerId` (`FriendPlayerId`),
  KEY `IX_player_friends_PlayerId` (`PlayerId`),
  CONSTRAINT `FK_player_friends_players_FriendPlayerId` FOREIGN KEY (`FriendPlayerId`) REFERENCES `players` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_player_friends_players_PlayerId` FOREIGN KEY (`PlayerId`) REFERENCES `players` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `player_friends`
--

/*!40000 ALTER TABLE `player_friends` DISABLE KEYS */;
/*!40000 ALTER TABLE `player_friends` ENABLE KEYS */;


--
-- Definition of table `player_items`
--

DROP TABLE IF EXISTS `player_items`;
CREATE TABLE `player_items` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `PlayerId` int(11) NOT NULL,
  `ShopItemInfoId` int(11) NOT NULL,
  `ShopPriceId` int(11) NOT NULL,
  `Color` tinyint(3) unsigned NOT NULL,
  `PurchaseDate` bigint(20) NOT NULL,
  `Durability` int(11) NOT NULL,
  `MP` int(11) NOT NULL DEFAULT '0',
  `MPLevel` int(11) NOT NULL DEFAULT '0',
  `Effects` longtext,
  PRIMARY KEY (`Id`),
  KEY `IX_player_items_PlayerId` (`PlayerId`),
  KEY `IX_player_items_ShopItemInfoId` (`ShopItemInfoId`),
  KEY `IX_player_items_ShopPriceId` (`ShopPriceId`),
  CONSTRAINT `FK_player_items_players_PlayerId` FOREIGN KEY (`PlayerId`) REFERENCES `players` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_player_items_shop_iteminfos_ShopItemInfoId` FOREIGN KEY (`ShopItemInfoId`) REFERENCES `shop_iteminfos` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_player_items_shop_prices_ShopPriceId` FOREIGN KEY (`ShopPriceId`) REFERENCES `shop_prices` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `player_items`
--

/*!40000 ALTER TABLE `player_items` DISABLE KEYS */;
/*!40000 ALTER TABLE `player_items` ENABLE KEYS */;


--
-- Definition of table `player_mails`
--

DROP TABLE IF EXISTS `player_mails`;
CREATE TABLE `player_mails` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `PlayerId` int(11) NOT NULL,
  `SenderPlayerId` int(11) NOT NULL,
  `SentDate` bigint(20) NOT NULL,
  `Title` varchar(100) NOT NULL,
  `Message` varchar(500) NOT NULL,
  `IsMailNew` bit(1) NOT NULL,
  `IsMailDeleted` bit(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_player_mails_PlayerId` (`PlayerId`),
  KEY `IX_player_mails_SenderPlayerId` (`SenderPlayerId`),
  CONSTRAINT `FK_player_mails_players_PlayerId` FOREIGN KEY (`PlayerId`) REFERENCES `players` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_player_mails_players_SenderPlayerId` FOREIGN KEY (`SenderPlayerId`) REFERENCES `players` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `player_mails`
--

/*!40000 ALTER TABLE `player_mails` DISABLE KEYS */;
/*!40000 ALTER TABLE `player_mails` ENABLE KEYS */;


--
-- Definition of table `player_settings`
--

DROP TABLE IF EXISTS `player_settings`;
CREATE TABLE `player_settings` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `PlayerId` int(11) NOT NULL,
  `Setting` varchar(100) NOT NULL,
  `Value` varchar(512) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_player_settings_PlayerId` (`PlayerId`),
  CONSTRAINT `FK_player_settings_players_PlayerId` FOREIGN KEY (`PlayerId`) REFERENCES `players` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `player_settings`
--

/*!40000 ALTER TABLE `player_settings` DISABLE KEYS */;
/*!40000 ALTER TABLE `player_settings` ENABLE KEYS */;


--
-- Definition of table `players`
--

DROP TABLE IF EXISTS `players`;
CREATE TABLE `players` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `TutorialState` tinyint(3) unsigned NOT NULL,
  `TotalExperience` int(11) NOT NULL,
  `PEN` int(11) NOT NULL,
  `AP` int(11) NOT NULL,
  `Coins1` int(11) NOT NULL,
  `Coins2` int(11) NOT NULL,
  `CurrentCharacterSlot` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `players`
--

/*!40000 ALTER TABLE `players` DISABLE KEYS */;
/*!40000 ALTER TABLE `players` ENABLE KEYS */;


--
-- Definition of table `shop_effect_groups`
--

DROP TABLE IF EXISTS `shop_effect_groups`;
CREATE TABLE `shop_effect_groups` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(20) NOT NULL,
  `PreviewEffect` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_shop_effect_groups_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `shop_effect_groups`
--

/*!40000 ALTER TABLE `shop_effect_groups` DISABLE KEYS */;
/*!40000 ALTER TABLE `shop_effect_groups` ENABLE KEYS */;


--
-- Definition of table `shop_effects`
--

DROP TABLE IF EXISTS `shop_effects`;
CREATE TABLE `shop_effects` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EffectGroupId` int(11) NOT NULL,
  `Effect` int(10) unsigned NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_shop_effects_EffectGroupId` (`EffectGroupId`),
  CONSTRAINT `FK_shop_effects_shop_effect_groups_EffectGroupId` FOREIGN KEY (`EffectGroupId`) REFERENCES `shop_effect_groups` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `shop_effects`
--

/*!40000 ALTER TABLE `shop_effects` DISABLE KEYS */;
/*!40000 ALTER TABLE `shop_effects` ENABLE KEYS */;


--
-- Definition of table `shop_iteminfos`
--

DROP TABLE IF EXISTS `shop_iteminfos`;
CREATE TABLE `shop_iteminfos` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ShopItemId` bigint(20) NOT NULL,
  `PriceGroupId` int(11) NOT NULL,
  `EffectGroupId` int(11) NOT NULL,
  `DiscountPercentage` tinyint(3) unsigned NOT NULL,
  `IsEnabled` bit(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_shop_iteminfos_EffectGroupId` (`EffectGroupId`),
  KEY `IX_shop_iteminfos_PriceGroupId` (`PriceGroupId`),
  KEY `IX_shop_iteminfos_ShopItemId` (`ShopItemId`),
  CONSTRAINT `FK_shop_iteminfos_shop_effect_groups_EffectGroupId` FOREIGN KEY (`EffectGroupId`) REFERENCES `shop_effect_groups` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_shop_iteminfos_shop_price_groups_PriceGroupId` FOREIGN KEY (`PriceGroupId`) REFERENCES `shop_price_groups` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_shop_iteminfos_shop_items_ShopItemId` FOREIGN KEY (`ShopItemId`) REFERENCES `shop_items` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `shop_iteminfos`
--

/*!40000 ALTER TABLE `shop_iteminfos` DISABLE KEYS */;
/*!40000 ALTER TABLE `shop_iteminfos` ENABLE KEYS */;


--
-- Definition of table `shop_items`
--

DROP TABLE IF EXISTS `shop_items`;
CREATE TABLE `shop_items` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `RequiredGender` tinyint(3) unsigned NOT NULL,
  `RequiredLicense` tinyint(3) unsigned NOT NULL,
  `Colors` tinyint(3) unsigned NOT NULL,
  `UniqueColors` tinyint(3) unsigned NOT NULL,
  `RequiredLevel` tinyint(3) unsigned NOT NULL,
  `LevelLimit` tinyint(3) unsigned NOT NULL,
  `RequiredMasterLevel` tinyint(3) unsigned NOT NULL,
  `IsOneTimeUse` bit(1) NOT NULL,
  `IsDestroyable` bit(1) NOT NULL,
  `MainTab` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `SubTab` tinyint(3) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `shop_items`
--

/*!40000 ALTER TABLE `shop_items` DISABLE KEYS */;
/*!40000 ALTER TABLE `shop_items` ENABLE KEYS */;


--
-- Definition of table `shop_price_groups`
--

DROP TABLE IF EXISTS `shop_price_groups`;
CREATE TABLE `shop_price_groups` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(20) NOT NULL,
  `PriceType` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_shop_price_groups_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `shop_price_groups`
--

/*!40000 ALTER TABLE `shop_price_groups` DISABLE KEYS */;
/*!40000 ALTER TABLE `shop_price_groups` ENABLE KEYS */;


--
-- Definition of table `shop_prices`
--

DROP TABLE IF EXISTS `shop_prices`;
CREATE TABLE `shop_prices` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `PriceGroupId` int(11) NOT NULL,
  `PeriodType` tinyint(3) unsigned NOT NULL,
  `Period` int(11) NOT NULL,
  `Price` int(11) NOT NULL,
  `IsRefundable` bit(1) NOT NULL,
  `Durability` int(11) NOT NULL,
  `IsEnabled` bit(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_shop_prices_PriceGroupId` (`PriceGroupId`),
  CONSTRAINT `FK_shop_prices_shop_price_groups_PriceGroupId` FOREIGN KEY (`PriceGroupId`) REFERENCES `shop_price_groups` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `shop_prices`
--

/*!40000 ALTER TABLE `shop_prices` DISABLE KEYS */;
/*!40000 ALTER TABLE `shop_prices` ENABLE KEYS */;


--
-- Definition of table `shop_version`
--

DROP TABLE IF EXISTS `shop_version`;
CREATE TABLE `shop_version` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Version` varchar(40) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `shop_version`
--

/*!40000 ALTER TABLE `shop_version` DISABLE KEYS */;
/*!40000 ALTER TABLE `shop_version` ENABLE KEYS */;


--
-- Definition of table `start_items`
--

DROP TABLE IF EXISTS `start_items`;
CREATE TABLE `start_items` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ShopItemInfoId` int(11) NOT NULL,
  `ShopPriceId` int(11) NOT NULL,
  `Color` tinyint(3) unsigned NOT NULL,
  `RequiredSecurityLevel` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_start_items_ShopItemInfoId` (`ShopItemInfoId`),
  KEY `IX_start_items_ShopPriceId` (`ShopPriceId`),
  CONSTRAINT `FK_start_items_shop_iteminfos_ShopItemInfoId` FOREIGN KEY (`ShopItemInfoId`) REFERENCES `shop_iteminfos` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_start_items_shop_prices_ShopPriceId` FOREIGN KEY (`ShopPriceId`) REFERENCES `shop_prices` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

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
