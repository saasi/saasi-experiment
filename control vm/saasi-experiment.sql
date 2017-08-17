/*
Navicat MySQL Data Transfer

Source Server         : localhost_3306
Source Server Version : 50718
Source Host           : localhost:3306
Source Database       : saasi-experiment

Target Server Type    : MYSQL
Target Server Version : 50718
File Encoding         : 65001

Date: 2017-07-24 13:37:27
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `bmsviolationcount`
-- ----------------------------
DROP TABLE IF EXISTS `bmsviolationcount`;
CREATE TABLE `bmsviolationcount` (
  `bmsGuid` varchar(100) NOT NULL,
  `violationCount` int(100) DEFAULT NULL,
  PRIMARY KEY (`bmsGuid`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of bmsviolationcount
-- ----------------------------
INSERT INTO `bmsviolationcount` VALUES ('b123456', '3');

-- ----------------------------
-- Table structure for `businessscaleout`
-- ----------------------------
DROP TABLE IF EXISTS `businessscaleout`;
CREATE TABLE `businessscaleout` (
  `vmAddress` varchar(20) NOT NULL,
  `lastScaleTime` datetime DEFAULT NULL,
  PRIMARY KEY (`vmAddress`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of businessscaleout
-- ----------------------------
INSERT INTO `businessscaleout` VALUES ('0:0:0:0:0:0:0:1', '2017-07-24 13:18:07');

-- ----------------------------
-- Table structure for `vmcount`
-- ----------------------------
DROP TABLE IF EXISTS `vmcount`;
CREATE TABLE `vmcount` (
  `vmAddress` varchar(20) NOT NULL,
  `bmsCount` int(11) DEFAULT NULL,
  PRIMARY KEY (`vmAddress`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of vmcount
-- ----------------------------
INSERT INTO `vmcount` VALUES ('0:0:0:0:0:0:0:1', '2');
