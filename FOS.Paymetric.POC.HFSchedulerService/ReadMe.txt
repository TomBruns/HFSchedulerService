dotnet publish -r win-x64 -c Release

To control Windows Services, the sc command can be used. Creating a new Windows Service is done using sc create passing the name of the service and the binPath parameter referencing the executable:
sc create “Sample Service” binPath=c:\sampleservice\WindowsServiceSample.exe

The status of the service can be queried using the Services MMC, or with the command line sc query:
sc query “Sample Service”

After the service is created, it is stopped and need to be started:
sc start “Sample Service”

sc start TestService
sc stop TestService
sc delete TestService