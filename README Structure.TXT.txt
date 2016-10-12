How is the directory structured:


CreateDatabase.SQL -> Create script for a new database


CentralServerService

Contains the implementation of the central server service (remoting) as well as CNDEP and MSMQ server parts. 


CentralServerService.OData

The ODATA service which can be hosted in IIS and uses the central server service


CentralServerService.Test

UnitTests (and usage examples) to test the core structures and .NET Remoting features of the server (basis for all other protocols)


CentralServiceLauncher

A launching application so that the server process can be launched properly (either as command line application or Windows service)


Common.Server

Common server side helper classes


DataStorageAccess

data access to SQL Server and abstract base classes


Device.PullSensorSimulator

a simulator application which can be used for query from a server (like a device) using HTTP GET requests


GatewayService.Test

several unit tests (and examples) to cover the various communication methods (CNDEP, MSMQ, REST, SOAP) with the server


GatewayServiceCommunicator

Test application for load testing the gateway 


GatewayServiceContract

The REST and SOAP implementations which are hosted in IIS and internally communicate with the CentralServerService


GlobalDataContracts

Global objects used in communication


Gsiot.Server.Simulator

"Getting Started with the Internet of Things" simulator base class which is used and extended by the DeviceServer.Simulator project (in solution DeviceServer.sln)



MsmqSensorTask

MSMQ client to send and receive sensor values


Tests

Contains various tests like ODATA, SQL, Workflow, Writing of actuator values and the core experiments as well as some raw result data from the experiments in 
Excel and text file format


ValueManagent

Classes need for value management 


ValueManagement.Test

test classes for the value management - callbacks, virtual value evaluation, trigger, load test, etc.







How to proceed the easiest for a fresh installation:


1) Take SQL Server 2012 Express Edition and create a database (the scripts assume Experiments as a name)
2) Create a SQL logon for a user EXPERIMENT with password EXPERIMENT (can be changed, yet is the current standard assumed everywhere)
3) run the script CreateDatabase.sql

4) Launch Visual Studio 2015 (free community edition) and load solution DeviceServer\DeviceServer.sln
5) rebuild all (will restore all missing packages from nuget)
6) launch the CentralServerServiceLauncher (should open a plain window)

7) launch another instance of visual studio 2015 and load Tests\Tests.Server.sln
8) Run the unit tests in CentralServerService.Tests -> especially RemotingTest

Comments:

If no .NET Micro Framework is installed, the loading of the solution DeviceServer.sln will cause warnings as some projects are then no longer compatible. 
This can be ignored as those are only needed for special occasions