CREATE DATABASE  IF NOT EXISTS `eltra_cloud` /*!40100 DEFAULT CHARACTER SET utf8 COLLATE utf8_unicode_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `eltra_cloud`;
-- MySQL dump 10.13  Distrib 8.0.15, for Win64 (x86_64)
--
-- Host: 192.168.1.22    Database: eltra_cloud
-- ------------------------------------------------------
-- Server version	8.0.16

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
 SET NAMES utf8 ;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Dumping data for table `command`
--

LOCK TABLES `command` WRITE;
/*!40000 ALTER TABLE `command` DISABLE KEYS */;
/*!40000 ALTER TABLE `command` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `command_parameter`
--

LOCK TABLES `command_parameter` WRITE;
/*!40000 ALTER TABLE `command_parameter` DISABLE KEYS */;
/*!40000 ALTER TABLE `command_parameter` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `data_type`
--

LOCK TABLES `data_type` WRITE;
/*!40000 ALTER TABLE `data_type` DISABLE KEYS */;
/*!40000 ALTER TABLE `data_type` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `device`
--

LOCK TABLES `device` WRITE;
/*!40000 ALTER TABLE `device` DISABLE KEYS */;
/*!40000 ALTER TABLE `device` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `device_lock`
--

LOCK TABLES `device_lock` WRITE;
/*!40000 ALTER TABLE `device_lock` DISABLE KEYS */;
/*!40000 ALTER TABLE `device_lock` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `device_user`
--

LOCK TABLES `device_user` WRITE;
/*!40000 ALTER TABLE `device_user` DISABLE KEYS */;
/*!40000 ALTER TABLE `device_user` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `device_version`
--

LOCK TABLES `device_version` WRITE;
/*!40000 ALTER TABLE `device_version` DISABLE KEYS */;
/*!40000 ALTER TABLE `device_version` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `exec_command`
--

LOCK TABLES `exec_command` WRITE;
/*!40000 ALTER TABLE `exec_command` DISABLE KEYS */;
/*!40000 ALTER TABLE `exec_command` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `exec_command_parameter`
--

LOCK TABLES `exec_command_parameter` WRITE;
/*!40000 ALTER TABLE `exec_command_parameter` DISABLE KEYS */;
/*!40000 ALTER TABLE `exec_command_parameter` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `location`
--

LOCK TABLES `location` WRITE;
/*!40000 ALTER TABLE `location` DISABLE KEYS */;
/*!40000 ALTER TABLE `location` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `parameter`
--

LOCK TABLES `parameter` WRITE;
/*!40000 ALTER TABLE `parameter` DISABLE KEYS */;
/*!40000 ALTER TABLE `parameter` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `parameter_value`
--

LOCK TABLES `parameter_value` WRITE;
/*!40000 ALTER TABLE `parameter_value` DISABLE KEYS */;
/*!40000 ALTER TABLE `parameter_value` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `session`
--

LOCK TABLES `session` WRITE;
/*!40000 ALTER TABLE `session` DISABLE KEYS */;
/*!40000 ALTER TABLE `session` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `session_command`
--

LOCK TABLES `session_command` WRITE;
/*!40000 ALTER TABLE `session_command` DISABLE KEYS */;
/*!40000 ALTER TABLE `session_command` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `session_devices`
--

LOCK TABLES `session_devices` WRITE;
/*!40000 ALTER TABLE `session_devices` DISABLE KEYS */;
/*!40000 ALTER TABLE `session_devices` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2019-06-23 21:37:15
