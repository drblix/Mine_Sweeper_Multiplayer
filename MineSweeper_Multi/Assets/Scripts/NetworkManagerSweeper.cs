using UnityEngine;
using Mirror;

public class NetworkManagerSweeper : NetworkManager
{
    private byte _connectionCount = 0;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        _connectionCount++;

        conn.identity.GetComponent<Player>().SetPlayerNum(conn, _connectionCount);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        _connectionCount--;

        foreach (NetworkConnectionToClient connection in NetworkServer.connections.Values)
        {
            if (connection != conn)
                connection.identity.GetComponent<Player>().SetPlayerNum(connection, _connectionCount);
        }
    }
}
