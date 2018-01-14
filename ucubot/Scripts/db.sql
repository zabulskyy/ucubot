CREATE DATABASE ucubot;
CREATE USER 'ucubot'@'localhost' IDENTIFIED BY '1qaz2wsx';
GRANT ALL PRIVILEGES ON ucubot.* TO 'ucubot'@'localhost';
FLUSH PRIVILEGES;