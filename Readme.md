# Petroineos.CodingChallenge.PowerTrader.IntradayReport

# Description
Traders require an intra-day report to give them their day ahead power position. The report
should output the aggregated volume per hour to a CSV file. Detailed requirements shared
in separate email.

#Technology
.Net core VS 2022 Version 6.0 
Language : c#
Unit test : Xunit

#Binaries - Contains external libraries and reference details.
Model - Intra report model placed in this section.
Services
Contract – Service contracts placed here
Implementation – Contracts implementation implemented here
Worker
Background service run the services based on the configured scheduled interval.
Logger and Error handling

#Reports
IntraRepot
CSV output file name generated and uploaded in this location.
TradeData
Trade files which are received from power service uploaded in this location. This helps for
verification and testing

#Appsettings configuration file sample values 
"ReportFileFormat": "yyyyMMdd_HHmm"
"ReportFileName": "PowerPosition_"
"ReportLocation": "./Reports/IntraReport/"
"ScheduledRunIntervalInMinutes": "2"
"IsRecordTradeData": true
"TradeDataLocation": "./Reports/TradeData/"
"VolumeFormat":"00.00"

#Windows service
Make this project as startup project -> Petroineos.CodingChallenge.PowerTrader.IntradayReport
Press F5 or press the run command

#Petroineos.CodingChallenge.PowerTrader.IntradayReport.Unitest
Go to Test menu and Press all the test cases