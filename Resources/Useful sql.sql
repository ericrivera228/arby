--Arbitration trades joined to arbitration run 
select 
	*
from 
	ARBITRATION_TRADE AT
	inner join ARBITRATION_RUN AR ON AT.ARBITRATION_RUN_ID = AR.ID