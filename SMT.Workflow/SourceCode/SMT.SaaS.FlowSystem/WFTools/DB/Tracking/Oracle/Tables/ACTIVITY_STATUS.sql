CREATE TABLE ACTIVITY_STATUS
(
	ACTIVITY_STATUS_ID NUMBER(4) NOT NULL
	,DESCRIPTION VARCHAR2(128) NOT NULL
);

ALTER TABLE ACTIVITY_STATUS ADD CONSTRAINT ACTIVITY_STATUS_PK PRIMARY KEY ( ACTIVITY_STATUS_ID );

INSERT INTO ACTIVITY_STATUS (ACTIVITY_STATUS_ID, DESCRIPTION)
VALUES (0, 'Initialised');
INSERT INTO ACTIVITY_STATUS (ACTIVITY_STATUS_ID, DESCRIPTION)
VALUES (1, 'Executing');
INSERT INTO ACTIVITY_STATUS (ACTIVITY_STATUS_ID, DESCRIPTION)
VALUES (2, 'Canceling');
INSERT INTO ACTIVITY_STATUS (ACTIVITY_STATUS_ID, DESCRIPTION)
VALUES (3, 'Closed');
INSERT INTO ACTIVITY_STATUS (ACTIVITY_STATUS_ID, DESCRIPTION)
VALUES (4, 'Compensating');
INSERT INTO ACTIVITY_STATUS (ACTIVITY_STATUS_ID, DESCRIPTION)
VALUES (5, 'Faulting');