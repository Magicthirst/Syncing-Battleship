# region CORE
from mcr.microsoft.com/dotnet/sdk:9.0 as build-core
workdir /app

copy Syncing_Battleship/Syncing_Battleship.csproj Syncing_Battleship/
copy Syncing_Battleship_Common_Typing/Syncing_Battleship_Common_Typing.csproj Syncing_Battleship_Common_Typing/
copy Syncing_Battleship_gRPC_Outlet/Syncing_Battleship_gRPC_Outlet.csproj Syncing_Battleship_gRPC_Outlet/
copy Util/Util.csproj Util/

run dotnet restore Syncing_Battleship/Syncing_Battleship.csproj
run dotnet restore Syncing_Battleship_Common_Typing/Syncing_Battleship_Common_Typing.csproj
run dotnet restore Syncing_Battleship_gRPC_Outlet/Syncing_Battleship_gRPC_Outlet.csproj
run dotnet restore Util/Util.csproj

copy Syncing_Battleship/. Syncing_Battleship/
copy Syncing_Battleship_Common_Typing/. Syncing_Battleship_Common_Typing/
copy Syncing_Battleship_gRPC_Outlet/. Syncing_Battleship_gRPC_Outlet/
copy Util/. Util/

run dotnet publish Syncing_Battleship/Syncing_Battleship.csproj -o /app/publish_exec
# endregion

# region DATA BEHAVIOUR
from mcr.microsoft.com/dotnet/sdk:9.0 as build-behaviour
workdir /app

copy Syncing_Battleship_Common_Typing/Syncing_Battleship_Common_Typing.csproj Syncing_Battleship_Common_Typing/
copy ["Magicthrist - Green/Magicthrist - Green.csproj", "Magicthrist - Green/"]

copy --from=build-core /root/.nuget/packages /root/.nuget/packages

run dotnet restore Syncing_Battleship_Common_Typing/Syncing_Battleship_Common_Typing.csproj
run dotnet restore "Magicthrist - Green/Magicthrist - Green.csproj"

copy Syncing_Battleship_Common_Typing/. Syncing_Battleship_Common_Typing/
copy ["Magicthrist - Green/.", "Magicthrist - Green/"]

run dotnet build "Magicthrist - Green/Magicthrist - Green.csproj" -c Release -o /app/build_dll
# endregion

# region FINAL
from mcr.microsoft.com/dotnet/sdk:9.0 as final
workdir /app

copy --from=build-core /app/publish_exec .
copy --from=build-behaviour /app/build_dll/Magicthrist___Green.dll .

# SYNC
expose 8765
# OUTLET
expose 8766

entrypoint ["dotnet", "Syncing_Battleship.dll"]
cmd ["Magicthrist___Green.dll", "Magicthrist___Green.MagicthirstDataBehaviour"]
# endregion
