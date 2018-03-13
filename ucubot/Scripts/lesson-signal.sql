USE ucubot;
create table lesson_signal(
  id INT (10) NOT NULL AUTO_INCREMENT, 
  Timestamp DateTime, 
  SignalType INT, 
  Userid VARCHAR (10),
  PRIMARY KEY (id));
