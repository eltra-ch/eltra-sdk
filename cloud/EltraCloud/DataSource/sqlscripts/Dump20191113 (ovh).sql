CREATE DATABASE  IF NOT EXISTS `eltra_cloud` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `eltra_cloud`;
-- MySQL dump 10.13  Distrib 8.0.17, for Win64 (x86_64)
--
-- Host: localhost    Database: eltra_cloud
-- ------------------------------------------------------
-- Server version	8.0.18

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `command`
--

DROP TABLE IF EXISTS `command`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `command` (
  `command_id` int(11) NOT NULL AUTO_INCREMENT,
  `device_idref` int(11) DEFAULT NULL,
  `name` varchar(255) CHARACTER SET utf8 COLLATE utf8_unicode_ci DEFAULT NULL,
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  `modified` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`command_id`),
  KEY `device_idref` (`device_idref`),
  KEY `device_name_idx` (`name`,`device_idref`),
  CONSTRAINT `command_ibfk_1` FOREIGN KEY (`device_idref`) REFERENCES `device` (`device_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `command_parameter`
--

DROP TABLE IF EXISTS `command_parameter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `command_parameter` (
  `command_parameter_id` int(11) NOT NULL AUTO_INCREMENT,
  `command_idref` int(11) DEFAULT NULL,
  `name` varchar(255) CHARACTER SET utf8 COLLATE utf8_unicode_ci DEFAULT NULL,
  `type` int(11) DEFAULT '0',
  `data_type_idref` int(11) DEFAULT NULL,
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  `modified` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`command_parameter_id`),
  KEY `command_idref` (`command_idref`),
  KEY `data_type_idref` (`data_type_idref`),
  KEY `command_type_name_idx` (`command_idref`,`data_type_idref`,`name`) USING BTREE,
  CONSTRAINT `command_parameter_ibfk_1` FOREIGN KEY (`command_idref`) REFERENCES `command` (`command_id`),
  CONSTRAINT `command_parameter_ibfk_2` FOREIGN KEY (`data_type_idref`) REFERENCES `data_type` (`data_type_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `data_type`
--

DROP TABLE IF EXISTS `data_type`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `data_type` (
  `data_type_id` int(11) NOT NULL AUTO_INCREMENT,
  `type` int(11) DEFAULT NULL,
  `size_bytes` int(11) DEFAULT NULL,
  `size_bits` int(11) DEFAULT NULL,
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  `modified` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`data_type_id`),
  KEY `type_sizes_idx` (`type`,`size_bytes`,`size_bits`) USING BTREE /*!80000 INVISIBLE */,
  KEY `type_idx` (`type`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `device`
--

DROP TABLE IF EXISTS `device`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `device` (
  `device_id` int(11) NOT NULL AUTO_INCREMENT,
  `serial_number` bigint(20) unsigned DEFAULT NULL,
  `device_name` varchar(255) CHARACTER SET utf8 COLLATE utf8_unicode_ci DEFAULT NULL,
  `product_name` varchar(255) CHARACTER SET utf8 COLLATE utf8_unicode_ci DEFAULT NULL,
  `device_version_idref` int(11) DEFAULT NULL,
  `status` int(11) DEFAULT NULL,
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  `modified` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`device_id`),
  KEY `device_version_idref` (`device_version_idref`),
  KEY `serial_number_idx` (`serial_number`) /*!80000 INVISIBLE */,
  KEY `status_idx` (`status`),
  CONSTRAINT `device_ibfk_1` FOREIGN KEY (`device_version_idref`) REFERENCES `device_version` (`device_version_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
--
-- Table structure for table `device_description`
--

DROP TABLE IF EXISTS `device_description`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `device_description` (
  `device_description_id` int(11) NOT NULL AUTO_INCREMENT,
  `device_version_idref` int(11) DEFAULT NULL,
  `content` mediumblob,
  `encoding` varchar(45) DEFAULT NULL,
  `hash_code` varchar(45) DEFAULT NULL,
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  `modified` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`device_description_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `device_lock`
--

DROP TABLE IF EXISTS `device_lock`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `device_lock` (
  `device_lock_id` int(11) NOT NULL AUTO_INCREMENT,
  `session_idref` int(11) NOT NULL,
  `device_idref` int(11) NOT NULL,
  `modified` datetime DEFAULT CURRENT_TIMESTAMP,
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`device_lock_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `device_tool`
--

DROP TABLE IF EXISTS `device_tool`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `device_tool` (
  `device_tool_id` int(11) NOT NULL AUTO_INCREMENT,
  `uuid` varchar(45) DEFAULT NULL,
  `name` varchar(45) DEFAULT NULL,
  `status` int(11) DEFAULT NULL,
  `modified` datetime DEFAULT CURRENT_TIMESTAMP,
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`device_tool_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `device_tool_set`
--

DROP TABLE IF EXISTS `device_tool_set`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `device_tool_set` (
  `device_tool_set_id` int(11) NOT NULL AUTO_INCREMENT,
  `device_idref` int(11) DEFAULT NULL,
  `device_tool_idref` int(11) DEFAULT NULL,
  `modified` datetime DEFAULT CURRENT_TIMESTAMP,
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`device_tool_set_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `device_user`
--

DROP TABLE IF EXISTS `device_user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `device_user` (
  `device_user_id` int(11) NOT NULL AUTO_INCREMENT,
  `login_name` varchar(255) CHARACTER SET utf8 COLLATE utf8_unicode_ci DEFAULT NULL,
  `user_name` varchar(255) CHARACTER SET utf8 COLLATE utf8_unicode_ci DEFAULT NULL,
  `password` varchar(255) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `status` int(11) DEFAULT NULL,
  `token` varchar(45) CHARACTER SET utf8 COLLATE utf8_unicode_ci DEFAULT NULL,
  `modified` datetime DEFAULT CURRENT_TIMESTAMP,
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`device_user_id`),
  UNIQUE KEY `login_name` (`login_name`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `device_version`
--

DROP TABLE IF EXISTS `device_version`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `device_version` (
  `device_version_id` int(11) NOT NULL AUTO_INCREMENT,
  `hardware_version` int(11) DEFAULT NULL,
  `software_version` int(11) DEFAULT NULL,
  `application_number` int(11) DEFAULT NULL,
  `application_version` int(11) DEFAULT NULL,
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  `modified` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`device_version_id`),
  KEY `versions_idx` (`hardware_version`,`software_version`,`application_number`,`application_version`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `exec_command`
--

DROP TABLE IF EXISTS `exec_command`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `exec_command` (
  `exec_command_id` int(11) NOT NULL AUTO_INCREMENT,
  `uuid` varchar(45) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `source_session_idref` int(11) DEFAULT NULL,
  `command_idref` int(11) DEFAULT NULL,
  `status` int(11) DEFAULT NULL,
  `communication_status` int(11) DEFAULT '0',
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  `modified` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`exec_command_id`),
  KEY `source_session_idref` (`source_session_idref`),
  KEY `command_idref` (`command_idref`),
  KEY `command_source_idx` (`source_session_idref`,`command_idref`),
  KEY `command_uuid_idx` (`uuid`),
  KEY `modified_idx` (`modified`,`communication_status`,`status`) USING BTREE,
  KEY `query_idx` (`source_session_idref`,`command_idref`,`modified`,`uuid`,`status`,`communication_status`),
  CONSTRAINT `exec_command_ibfk_1` FOREIGN KEY (`source_session_idref`) REFERENCES `session` (`session_id`),
  CONSTRAINT `exec_command_ibfk_2` FOREIGN KEY (`command_idref`) REFERENCES `command` (`command_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `exec_command_parameter`
--

DROP TABLE IF EXISTS `exec_command_parameter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `exec_command_parameter` (
  `exec_command_parameter_id` int(11) NOT NULL AUTO_INCREMENT,
  `exec_command_idref` int(11) DEFAULT NULL,
  `command_parameter_idref` int(11) DEFAULT NULL,
  `value` blob,
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  `modified` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`exec_command_parameter_id`),
  KEY `exec_command_idref` (`exec_command_idref`),
  KEY `command_parameter_idref` (`command_parameter_idref`),
  KEY `exec_command_param_idx` (`exec_command_idref`,`command_parameter_idref`),
  CONSTRAINT `exec_command_parameter_ibfk_1` FOREIGN KEY (`exec_command_idref`) REFERENCES `exec_command` (`exec_command_id`),
  CONSTRAINT `exec_command_parameter_ibfk_2` FOREIGN KEY (`command_parameter_idref`) REFERENCES `command_parameter` (`command_parameter_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `location`
--

DROP TABLE IF EXISTS `location`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `location` (
  `location_id` int(11) NOT NULL AUTO_INCREMENT,
  `ip` varchar(45) CHARACTER SET utf8 COLLATE utf8_unicode_ci DEFAULT NULL,
  `country_code` varchar(45) CHARACTER SET utf8 COLLATE utf8_unicode_ci DEFAULT NULL,
  `country` varchar(45) CHARACTER SET utf8 COLLATE utf8_unicode_ci DEFAULT NULL,
  `region` varchar(45) CHARACTER SET utf8 COLLATE utf8_unicode_ci DEFAULT NULL,
  `city` varchar(45) CHARACTER SET utf8 COLLATE utf8_unicode_ci DEFAULT NULL,
  `latitude` double DEFAULT NULL,
  `longitude` double DEFAULT NULL,
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  `modified` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`location_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parameter`
--

DROP TABLE IF EXISTS `parameter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `parameter` (
  `parameter_id` int(11) NOT NULL AUTO_INCREMENT,
  `device_id` int(11) DEFAULT NULL,
  `unique_id` varchar(255) CHARACTER SET utf8 COLLATE utf8_unicode_ci DEFAULT NULL,
  `index` int(11) DEFAULT NULL,
  `sub_index` int(11) DEFAULT NULL,
  `data_type_idref` int(11) DEFAULT NULL,
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`parameter_id`),
  KEY `device_id` (`device_id`),
  KEY `device_unique_idx` (`device_id`,`unique_id`),
  CONSTRAINT `parameter_ibfk_1` FOREIGN KEY (`device_id`) REFERENCES `device` (`device_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parameter_value`
--

DROP TABLE IF EXISTS `parameter_value`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `parameter_value` (
  `parameter_value_id` int(11) NOT NULL AUTO_INCREMENT,
  `parameter_idref` int(11) DEFAULT NULL,
  `actual_value` blob,
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`parameter_value_id`),
  KEY `parameter_idref` (`parameter_idref`),
  CONSTRAINT `parameter_value_ibfk_1` FOREIGN KEY (`parameter_idref`) REFERENCES `parameter` (`parameter_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `session`
--

DROP TABLE IF EXISTS `session`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `session` (
  `session_id` int(11) NOT NULL AUTO_INCREMENT,
  `uuid` varchar(127) CHARACTER SET utf8 COLLATE utf8_unicode_ci DEFAULT NULL,
  `device_user_idref` int(11) DEFAULT NULL,
  `location_idref` int(11) NOT NULL,
  `status` int(11) DEFAULT NULL,
  `modified` datetime DEFAULT CURRENT_TIMESTAMP,
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`session_id`),
  UNIQUE KEY `uuid` (`uuid`) /*!80000 INVISIBLE */,
  KEY `device_user_idref` (`device_user_idref`),
  KEY `uuid_user_idx` (`uuid`,`device_user_idref`) USING BTREE,
  KEY `location_fk_idx` (`location_idref`),
  KEY `modified_status_idx` (`modified`,`status`,`uuid`),
  CONSTRAINT `location_fk` FOREIGN KEY (`location_idref`) REFERENCES `location` (`location_id`),
  CONSTRAINT `session_ibfk_1` FOREIGN KEY (`device_user_idref`) REFERENCES `device_user` (`device_user_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `session_command`
--

DROP TABLE IF EXISTS `session_command`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `session_command` (
  `session_command_id` int(11) NOT NULL AUTO_INCREMENT,
  `session_idref` int(11) DEFAULT NULL,
  `command_idref` int(11) DEFAULT NULL,
  `status` int(11) DEFAULT '0',
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  `modified` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`session_command_id`),
  KEY `session_idref` (`session_idref`),
  KEY `command_idref` (`command_idref`),
  KEY `session_command_idx` (`session_idref`,`command_idref`) USING BTREE,
  CONSTRAINT `session_command_ibfk_1` FOREIGN KEY (`session_idref`) REFERENCES `session` (`session_id`),
  CONSTRAINT `session_command_ibfk_2` FOREIGN KEY (`command_idref`) REFERENCES `command` (`command_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `session_devices`
--

DROP TABLE IF EXISTS `session_devices`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `session_devices` (
  `session_devices_id` int(11) NOT NULL AUTO_INCREMENT,
  `session_idref` int(11) DEFAULT NULL,
  `device_idref` int(11) DEFAULT NULL,
  `created` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`session_devices_id`),
  KEY `session_idref` (`session_idref`),
  KEY `device_idref` (`device_idref`),
  KEY `session_device_idx` (`session_idref`,`device_idref`) USING BTREE,
  CONSTRAINT `session_devices_ibfk_1` FOREIGN KEY (`session_idref`) REFERENCES `session` (`session_id`),
  CONSTRAINT `session_devices_ibfk_2` FOREIGN KEY (`device_idref`) REFERENCES `device` (`device_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2019-11-13  5:33:48
