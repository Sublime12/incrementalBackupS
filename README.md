# incrementalBackupS

I call this project incrementalBackup because i'm creating cli application in dotnet-7 
which goal is to make incremental backup of a file system (i'm first targetting linux) 

This project is a solution divided which has 3 projects :
- Cli : Is be the cli interface of my application, I'm using the nuget package System.CommandLine to speed up my developpment time

- Compresser : Is the project which role is to compress a directory

    My first idea for this project is to create a deamon service waiting for a message sent by the Cli interface to execute some compression action

- FilesystemTracker : Is the management tool which role is to versionning every backup,
