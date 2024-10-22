DELIMITER $$

DROP PROCEDURE IF EXISTS InsertEventAnnotations $$
/*
 * Insert a batch of event annotations.
 */	
CREATE PROCEDURE InsertEventAnnotations
(
	IN p_WORKFLOW_INSTANCE_ID BIGINT UNSIGNED
	,IN p_EVENT_TYPE CHAR(1)
	,IN p_EVENT_ID_1 BIGINT UNSIGNED
	,IN p_ANNOTATION_1 VARCHAR(1024)
	,IN p_EVENT_ID_2 BIGINT UNSIGNED
	,IN p_ANNOTATION_2 VARCHAR(1024)
	,IN p_EVENT_ID_3 BIGINT UNSIGNED
	,IN p_ANNOTATION_3 VARCHAR(1024)
	,IN p_EVENT_ID_4 BIGINT UNSIGNED
	,IN p_ANNOTATION_4 VARCHAR(1024)
	,IN p_EVENT_ID_5 BIGINT UNSIGNED
	,IN p_ANNOTATION_5 VARCHAR(1024)
)
sproc:BEGIN
	-- parameter set 1
	CALL InsertEventAnnotation(p_WORKFLOW_INSTANCE_ID, 
		p_EVENT_ID_1, p_EVENT_TYPE, p_ANNOTATION_1);
		
	-- parameter set 2
	IF p_EVENT_ID_2 IS NULL THEN
		LEAVE sproc;
	END IF;

	CALL InsertEventAnnotation(p_WORKFLOW_INSTANCE_ID, 
		p_EVENT_ID_2, p_EVENT_TYPE, p_ANNOTATION_2);
		
	-- parameter set 3
	IF p_EVENT_ID_3 IS NULL THEN
		LEAVE sproc;
	END IF;

	CALL InsertEventAnnotation(p_WORKFLOW_INSTANCE_ID, 
		p_EVENT_ID_3, p_EVENT_TYPE, p_ANNOTATION_3);

	-- parameter set 4
	IF p_EVENT_ID_4 IS NULL THEN
		LEAVE sproc;
	END IF;

	CALL InsertEventAnnotation(p_WORKFLOW_INSTANCE_ID, 
		p_EVENT_ID_4, p_EVENT_TYPE, p_ANNOTATION_4);

	-- parameter set 5
	IF p_EVENT_ID_5 IS NULL THEN
		LEAVE sproc;
	END IF;

	CALL InsertEventAnnotation(p_WORKFLOW_INSTANCE_ID, 
		p_EVENT_ID_5, p_EVENT_TYPE, p_ANNOTATION_5);
END $$

DELIMITER ;
