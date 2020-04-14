Recurring Job Schedule Field

Cron expression reference
	https://github.com/HangfireIO/Cronos
	https://en.wikipedia.org/wiki/Cron#CRON_expression

Cron Field String Builder
	http://www.cronmaker.com/

	minute hour dom month dow 

	minute	This controls what minute of the hour the command will run on, and is between '0' and '59'
	hour	This controls what hour the command will run on, and is specified in the 24 hour clock, values must be between 0 and 23 (0 is midnight)
	dom	This is the Day of Month, that you want the command run on, e.g. to run a command on the 19th of each month, the dom would be 19.
	month	This is the month a specified command will run on, it may be specified numerically (0-12), or as the name of the month (e.g. May)
	dow	This is the Day of Week that you want a command to be run on, it can also be numeric (0-7) or as the name of the day (e.g. sun).
	
										   Allowed values    Allowed special characters   Comment

	┌───────────── second (optional)       0-59              * , - /                      
	│ ┌───────────── minute                0-59              * , - /                      
	│ │ ┌───────────── hour                0-23              * , - /                      
	│ │ │ ┌───────────── day of month      1-31              * , - / L W ?                
	│ │ │ │ ┌───────────── month           1-12 or JAN-DEC   * , - /                      
	│ │ │ │ │ ┌───────────── day of week   0-6  or SUN-SAT   * , - / # L ?                Both 0 and 7 means SUN
	│ │ │ │ │ │
	* * * * * *

Hangfire Documentation

	http://docs.hangfire.io/en/latest/background-methods/performing-recurrent-tasks.html