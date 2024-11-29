## Contents and Quick Links

- [Unity Game Services (UGS)](ugs.md)
- [Configuring Multiplay Hosting](#configuring-multiplay-hosting)
- [Testing game servers](#configuring-multiplay-hosting)

> **Note**: Multiplay Hosting (formerly known as Game Server Hosting) is a pay-as-you-go service with a free tier so you must be an Owner or Manager of your organization to enable Multiplay Hosting by signing up with a credit card.
>
> If you exceed the [free tier usage allowance](https://unity.com/solutions/gaming-services/pricing), you will be charged. See our [Billing FAQ](https://support.unity.com/hc/en-us/articles/6821475035412-Billing-FAQ) to learn more.

**Multiplay Hosting** is a robust and flexible infrastructure for hosting multiplayer games. It ensures a smooth operation of your game by delivering proven performance and scalability. With Multiplay Hosting, you can launch your multiplayer titles with confidence, knowing that you have the support of a reliable global platform. The enables you to spend less time troubleshooting and more time building your game with the help of comprehensive documentation, samples, and software development kits (SDKs) designed for interoperability. [Get started with Multiplay Hosting](https://docs.unity.com/game-server-hosting/en/manual/guides/get-started).

To use Multiplay Hosting in your project, you need to [Integrate the Multiplay Hosting](https://docs.unity.com/game-server-hosting/manual/guides/get-started#Integrat) service from [Unity Cloud](https://cloud.unity.com/home).

## Configuring Multiplay Hosting

> **Tip**: Check out our YouTube video [How to set up Multiplay Hosting](https://www.youtube.com/watch?v=oN2c9teXi7M).

We recommend [using the default configurations for development](./ugs.md#multiplay-hosting-game-server-hosting) to get started quickly with Megacity Metro.

Alternatively, you can [get started to manually configure](https://docs.unity.com/ugs/en-us/manual/game-server-hosting/manual/guides/get-started#Create) a Build, Build configuration, Fleet, and Test Allocation.

Default configuration files for Multiplay Hosting are located in the `Settings/UGS` directory with relevant settings found in the `.gsh` file.

Fleet properties are configured for development, specifically `minAvailable: 0` which will remove servers after a period of inactivity to limit unexpected usage.
```
fleets:
  megacity-metro-fleet: # replace with the name for your fleet
    ...
    regions:
      North America: # North America, Europe, Asia, South America, Australia
        minAvailable: 0 # minimum number of servers running in the region
        maxServers: 1 # maximum number of servers running in the region
```

Machines without any servers will also be deleted after a period of time.

## Testing game servers

### Querying game servers

Multiplay Hosting queries game servers to determine which are active and which can safely be removed, based on current players. Megacity Metro is configured to use [Server Query Protocol (SQP)](https://docs.unity.com/ugs/en-us/manual/game-server-hosting/manual/concepts/sqp#standard_query_response), with the Multiplayer Services SDK automatically starting an SQP service on the server.

Once a server is made **available** through the [allocation lifecycle](https://docs.unity.com/ugs/en-us/manual/game-server-hosting/manual/concepts/allocation-lifecycle), you can test this using a CLI provided in the [go-svrquery](https://github.com/Unity-Technologies/go-svrquery/releases/) project.

Determine the server's IP in the dashboard and then run: `./go-svrquery -addr <ip>:9010 -proto sqp`

You should see a result like:
```
{
        "version": 1,
        "address": "<ip>:9010",
        "server_info": {
                "current_players": 1,
                "max_players": 200,
                "server_name": "Megacity Multiplayer Server",
                "game_type": "Arena",
                "build_id": "e6e446fd895c4bf681919979bb22ebb8",
                "map": "Megacity",
                "port": 0
        }
}
```

The `port` value will be set to zero (0) by default until the server is **allocated** and will then indicate the actual connection port for game clients.

### Connection errors

As a result of the `minAvailable` server property described in [Configuring Multiplay Hosting](#configuring-multiplay-hosting), initial attempts connecting to a Multiplay Hosting server may timeout after a long period of inactivity. This will continue while waiting for a machine to boot and then for a server to start.

Learm more about the [game server lifecycle](https://docs.unity.com/ugs/en-us/manual/game-server-hosting/manual/concepts/server-lifecycle).
