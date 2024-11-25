version: 1.0
builds:
  megacity-metro-matchmaker: # replace with the name for your build
    executableName: Server.x86_64 # the name of your build executable
    buildPath: Builds/Multiplay # the location of the build files
    excludePaths: # paths to exclude from upload (supports only basic patterns)
      - Builds/Multiplay/DoNotShip/*.*
      - Builds/Multiplay/Server_BackUpThisFolder_ButDontShipItWithYourGame/*.*
buildConfigurations:
  megacity-metro-build-config: # replace with the name for your build configuration
    build: megacity-metro-matchmaker # replace with the name for your build
    queryType: sqp # sqp or a2s, delete if you do not have logs to query
    binaryPath: Server.x86_64 # the name of your build executable
    commandLine: -ip $$ip$$ -port $$port$$ -queryport $$query_port$$ -logFile $$log_dir$$/$$timestamp$$-Engine.log # launch parameters for your server
    variables: {}
    cores: 2 # number of cores per server
    speedMhz: 1500 # launch parameters for your server
    memoryMiB: 7000 # launch parameters for your server
fleets:
  megacity-metro-fleet: # replace with the name for your fleet
    buildConfigurations:
      - megacity-metro-build-config # replace with the names of your build configuration
    regions:
      North America: # North America, Europe, Asia, South America, Australia
        minAvailable: 0 # minimum number of servers running in the region
        maxServers: 1 # maximum number of servers running in the region
