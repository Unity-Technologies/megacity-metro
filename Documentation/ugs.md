## Contents and Quick Links

- [Game Server Hosting (Multiplay)](#game-server-hosting-multiplay)
- [Matchmaker](#matchmaker)
- [Vivox](#vivox)

## Add Unity Gaming Services (UGS)

Megacity-Metro uses several services from UGS to facilitate connectivity between players. To use these services inside your project, you need a [Unity Account](https://docs.unity.com/ugs-overview/en/manual/creating-unity-ids) and [create an organization](https://support.unity.com/hc/en-us/articles/208592876-How-do-I-create-a-new-Organization-) within the Unity Dashboard.

You can still use Megacity-Metro without UGS, but for a better multiplayer experience, it is recommended to use the following services:

### Game Server Hosting (Multiplay)

**Game Server Hosting**, formerly known as Multiplay, is a robust and flexible infrastructure for hosting multiplayer games. It ensures a smooth operation of your game by delivering proven performance and scalability. With Game Server Hosting, you can launch your multiplayer titles with confidence, knowing that you have the support of a reliable global platform. The enables you to spend less time troubleshooting and more time building your game with the help of comprehensive documentation, samples, and software development kits (SDKs) designed for interoperability. To get started with Game Server Hosting, refer to the official [documentation](https://docs.unity.com/game-server-hosting/en/manual/guides/get-started).

**Warning**: Game Server Hosting is a pay-as-you-go service with a free tier. You must sign up for UGS services with a credit card to start using Game Server Hosting. If you exceed the [free tier usage allowance](https://unity.com/solutions/gaming-services/pricing), you will be charged. See our [Billing FAQ](https://support.unity.com/hc/en-us/articles/6821475035412-Billing-FAQ) to learn more.

To use Game Server Hosting in your project, you need to [Integrate the Game Server Hosting](https://docs.unity.com/game-server-hosting/manual/guides/get-started#Integrat) service from the [Unity Cloud](https://cloud.unity.com/home).

**Note**: You must be an Owner or Manager of your organization to enable Game Server Hosting.

After you integrate Game Server Hosting, you must create a [build](https://docs.unity.com/game-server-hosting/manual/guides/get-started#Create), a [build configuration](https://docs.unity.com/game-server-hosting/manual/guides/get-started#Create2), a [fleet](https://docs.unity.com/game-server-hosting/manual/guides/get-started#Create3), and a [test allocation](https://docs.unity.com/game-server-hosting/manual/guides/get-started#Create4).

**Tip**: Check out our YouTube video [How to set up Game Server Hosting](https://www.youtube.com/watch?v=oN2c9teXi7M).

For Megacity-Metro, we use the following Game Server Hosting setting:

For **Build Configuration**: 

- **Query Type**: `SQP`
- **Launch parameters**: `-ip $$ip$$ -port $$port$$ -queryport $$query_port$$ -logFile $$log_dir$$/$$timestamp$$-Engine.log`

For **Fleet**: 
- **Server Per Machine** : `1` 

### Matchmaker

**Matchmaker** is a versatile tool that enables you to customize matches in your game. It offers fast and efficient matches, multi-region orchestration, and backfill options. With its flexible configuration, dynamic scalability, and robust rule engine, Matchmaker simplifies matchmaking while supporting complex game loops. For more information, consult the [Matchmaker Quick Start Guide](https://docs.unity.com/matchmaker/en/manual/matchmaker-quick-start).

To use Matchmaker in your project, you must **Enable** and **Integrate** the Matchmaker service from the [Unity Cloud](https://cloud.unity.com/home).

For Megacity-Metro, we use the following Matchmaker configuration:

Creating the [queue](https://docs.unity.com/matchmaker/en/manual/advanced-topics-queues-pools#Queues):
- **Maximum players on a ticket**: 12

Creating a default [pool](https://docs.unity.com/matchmaker/en/manual/advanced-topics-queues-pools#Pools):
- **Timeout**: 60 seconds

For Matchmaker [rules](https://docs.unity.com/matchmaker/manual/matchmaking-rules-rules), we use the following configuration:
- **Backfill enabled**: true 
- **Team count min**: 1
- **Team count max**: 1
- **Player count min**: 200
- **Player count max**: 200
- **Relaxation 1**: 
  - **Range Control** : Replace min 
  - **Ticket age tyep** : Oldest
  - **Replacement value** : 1
  - **At seconds** : 10

After configuring the services on the dashboard website, navigate to **Edit > Project Settings > Service** and choose your organization and project ID.

![Project ID](../Readme/setting-project-id.png)

Next, click on the play button to initiate the game. To access the Matchmaking services, navigate to the main menu and select **"Matchmake"** mode, followed by clicking the **"Find Match"** button.

![Selecting Matchmaking](../Readme/selecting-matchmaking.png)

Once the "Find Match" button is clicked, the Matchmaking services will initiate the connection process with the server. During this time, a circular loading indicator will be presented to signify that the system is in the process of establishing the connection. Once the connection is established, you will be able to start gameplay.

![Selecting Matchmaking](../Readme/establishing-connection.png)


### Vivox

**Vivox** is a voice chat service that enables players to communicate with each other in-game. To use [Vivox](https://unity.com/products/vivox), you need to connect your project to Vivox from the Unity Editor and enable Vivox in the [Unity Cloud](https://cloud.unity.com/home).

For more information about Vivox, and how to use it you can read the [Vivox quickstart guide](https://docs.vivox.com/v5/general/unity/15_1_200000/en-us/Default.htm#Unity/vivox-unity-first-steps.htm).