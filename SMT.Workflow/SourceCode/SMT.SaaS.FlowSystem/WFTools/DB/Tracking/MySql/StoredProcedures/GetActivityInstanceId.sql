DELIMITER $$

DROP PROCEDURE IF EXISTS GetActivityInstanceId $$
/*
 * Retrieve the unique identifier of an activity instance, creating a new one
 * if necessary.
 */
CREATE PROCEDURE GetActivityInstanceId
(
	IN p_WORKFLOW_INSTANCE_ID BIGINT UNSIGNED
	,IN p_QUALIFIED_NAME VARCHAR(128)
	,IN p_CONTEXT_GUID CHAR(36)
	,IN p_PARENT_CONTEXT_GUID CHAR(36)
	,OUT p_ACTIVITY_INSTANCE_ID BIGINT UNSIGNED
)
BEGIN
	DECLARE l_WORKFLOW_INSTANCE_EVENT_ID BIGINT UNSIGNED;
	
	SELECT
		ACTIVITY_INSTANCE_ID
	INTO
		p_ACTIVITY_INSTANCE_ID
	FROM
		ACTIVITY_INSTANCE
	WHERE
		WORKFLOW_INSTANCE_ID = p_WORKFLOW_INSTANCE_ID
		AND QUALIFIED_NAME = p_QUALIFIED_NAME
		AND CONTEXT_GUID = p_CONTEXT_GUID
		AND PARENT_CONTEXT_GUID = p_PARENT_CONTEXT_GUID;
	
	IF p_ACTIVITY_INSTANCE_ID IS NULL THEN
		
		-- if a record exists in ADDED_ACTIVITY for this qualified name
		-- then use the workflow event id when inserting this record
		SELECT
			MAX(WORKFLOW_INSTANCE_EVENT_ID)
		INTO
			l_WORKFLOW_INSTANCE_EVENT_ID
		FROM
			ADDED_ACTIVITY
		WHERE
			QUALIFIED_NAME = p_QUALIFIED_NAME;
			
		INSERT INTO ACTIVITY_INSTANCE
		(
			WORKFLOW_INSTANCE_ID
			,QUALIFIED_NAME
			,CONTEXT_GUID
			,PARENT_CONTEXT_GUID
			,WORKFLOW_INSTANCE_EVENT_ID
		)
		VALUES
		(
			p_WORKFLOW_INSTANCE_ID
			,p_QUALIFIED_NAME
			,p_CONTEXT_GUID
			,p_PARENT_CONTEXT_GUID
			,l_WORKFLOW_INSTANCE_EVENT_ID
		);
		
		SET p_ACTIVITY_INSTANCE_ID = LAST_INSERT_ID();
	END IF;
END $$

DELIMITER ;