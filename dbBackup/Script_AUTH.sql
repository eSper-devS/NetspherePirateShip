DROP TABLE IF EXISTS __efmigrationshistory;

CREATE TABLE __efmigrationshistory (
  MigrationId TEXT NOT NULL,
  ProductVersion TEXT NOT NULL,
  PRIMARY KEY (MigrationId)
);
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;


--
-- Definition of table `accounts`
--

DROP TABLE IF EXISTS accounts;

CREATE TABLE accounts (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Username TEXT NOT NULL,
  Nickname TEXT NULL,
  Password TEXT NULL,
  Salt TEXT NULL,
  SecurityLevel INTEGER NOT NULL
);

CREATE UNIQUE INDEX IX_accounts_Username ON accounts (Username);
CREATE UNIQUE INDEX IX_accounts_Nickname ON accounts (Nickname);

--
-- Dumping data for table `accounts`
--

/*!40000 ALTER TABLE `accounts` DISABLE KEYS */;
/*!40000 ALTER TABLE `accounts` ENABLE KEYS */;


--
-- Definition of table `bans`
--

DROP TABLE IF EXISTS bans;

CREATE TABLE bans (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  AccountId INTEGER NOT NULL,
  Date INTEGER NOT NULL,
  Duration INTEGER NULL,
  Reason TEXT NULL,
  FOREIGN KEY (AccountId) REFERENCES accounts (Id) ON DELETE CASCADE
);

CREATE INDEX IX_bans_AccountId ON bans (AccountId);

--
-- Dumping data for table `bans`
--

/*!40000 ALTER TABLE `bans` DISABLE KEYS */;
/*!40000 ALTER TABLE `bans` ENABLE KEYS */;


--
-- Definition of table `login_history`
--

DROP TABLE IF EXISTS login_history;

CREATE TABLE login_history (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  AccountId INTEGER NOT NULL,
  Date INTEGER NOT NULL,
  IP TEXT NOT NULL,
  FOREIGN KEY (AccountId) REFERENCES accounts (Id) ON DELETE CASCADE
);

CREATE INDEX IX_login_history_AccountId ON login_history (AccountId);

--
-- Dumping data for table `login_history`
--

/*!40000 ALTER TABLE `login_history` DISABLE KEYS */;
/*!40000 ALTER TABLE `login_history` ENABLE KEYS */;


--
-- Definition of table `nickname_history`
--

DROP TABLE IF EXISTS nickname_history;

CREATE TABLE nickname_history (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  AccountId INTEGER NOT NULL,
  Nickname TEXT NOT NULL,
  ExpireDate INTEGER NULL,
  FOREIGN KEY (AccountId) REFERENCES accounts (Id) ON DELETE CASCADE
);

CREATE INDEX IX_nickname_history_AccountId ON nickname_history (AccountId);
--
-- Dumping data for table `nickname_history`
--

/*!40000 ALTER TABLE `nickname_history` DISABLE KEYS */;
/*!40000 ALTER TABLE `nickname_history` ENABLE KEYS */;

--
-- Create schema game
--


