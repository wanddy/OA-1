DELIMITER $$

DROP PROCEDURE IF EXISTS InsertWorkflowInstance $$
/*
 * Insert a workflow instance record.
 */	
CREATE PROCEDURE InsertWorkflowInstance
(
	IN p_INSTANCE_ID CHAR(36)
	,IN p_TYPE_FULL_NAME VARCHAR(128)
	,IN p_ASSEMBLY_FULL_NAME VARCHAR(256)
	,IN p_CONTEXT_GUID CHAR(36)
	,IN p_CALLER_INSTANCE_ID CHAR(36)
	,IN p_CALL_PATH VARCHAR(400)
	,IN p_CALLER_CONTEXT_GUID CHAR(36)
	,IN p_CALLER_PARENT_CONTEXT_GUID CHAR(36)
	,IN p_INITIALISED_DATE_TIME DATETIME
)
BEGIN
	DECLARE l_WORKFLOW_TYPE_ID BIGINT UNSIGNED;
	DECLARE l_WORKFLOW_INSTANCE_ID BIGINT UNSIGNED;
	
	CALL GetTypeId(l_WORKFLOW_TYPE_ID, p_TYPE_FULL_NAME, p_ASSEMBLY_FULL_NAME, NULL);
	
	SELECT
		WORKFLOW_INSTANCE_ID
	INTO
		l_WORKFLOW_INSTANCE_ID
	FROM
		WORKFLOW_INSTANCE
	WHERE
		WORKFLOW_TYPE_ID = l_WORKFLOW_TYPE_ID
		AND INSTANCE_ID = p_INSTANCE_ID;
		
	IF l_WORKFLOW_INSTANCE_ID IS NULL THEN
		INSERT INTO WORKFLOW_INSTANCE
		(
			INSTANCE_ID
			,CONTEXT_GUID
			,CALLER_INSTANCE_ID
			,CALL_PATH
			,CALLER_CONTEXT_GUID
			,CALLER_PARENT_CONTEXT_GUID
			,WORKFLOW_TYPE_ID
			,INITIALISED_DATE_TIME
			,DB_INITIALISED_DATE_TIME
		)
		VALUES
		(
			p_INSTANCE_ID
			,p_CONTEXT_GUID
			,p_CALLER_INSTANCE_ID
			,p_CALL_PATH
			,p_CALLER_CONTEXT_GUID
			,p_CALLER_PARENT_CONTEXT_GUID
			,l_WORKFLOW_TYPE_ID
			,p_INITIALISED_DATE_TIME
			,UTC_TIMESTAMP()
		);
		
		SET l_WORKFLOW_INSTANCE_ID = LAST_INSERT_ID();
	END IF;
	
	SELECT l_WORKFLOW_INSTANCE_ID;
END $$

DELIMITER ;
