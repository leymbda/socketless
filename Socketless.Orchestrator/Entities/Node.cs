using Socketless.Orchestrator.Common;
using System.Collections.ObjectModel;
using System.Net;

namespace Socketless.Orchestrator.Entities;

public class Node
{
    public const int MaximumShards = 300;
    public const float InitialBalance = 1000;

    private readonly Dictionary<ShardId, Shard> _shards = [];
    private readonly Dictionary<IPAddress, IPAddress> _ipAddresses;
    private readonly IPAddress _primaryIpAddress;

    public InstanceId InstanceId { get; init; }

    public IReadOnlyDictionary<ShardId, Shard> Shards { get; }

    public IReadOnlyDictionary<IPAddress, IPAddress> IpAddresses { get; }

    public DateTime CreatedAt { get; init; }

    public int Space => MaximumShards - Shards.Count;

    public float Balance => InitialBalance - Shards.Values.Sum(s => s.Cost);

    public Node(InstanceId instanceId, IPAddress primaryPublicIp, IPAddress primaryPrivateIp, DateTime createdAt)
    {
        InstanceId = instanceId;
        CreatedAt = createdAt;
        _primaryIpAddress = primaryPublicIp;
        _ipAddresses = new() { [primaryPublicIp] = primaryPrivateIp };

        Shards = new ReadOnlyDictionary<ShardId, Shard>(_shards);
        IpAddresses = new ReadOnlyDictionary<IPAddress, IPAddress>(_ipAddresses);
    }

    /// <summary>
    /// Add a shard to the node.
    /// </summary>
    /// <param name="shard">The shard to be added</param>
    /// <exception cref="InvalidOperationException">Occurs if the node cannot accommodate the shard</exception>
    public void AddShard(Shard shard)
    {
        if (Space == 0)
            throw new InvalidOperationException("Insufficient space to add another shard");

        if (Balance - shard.Cost < 0)
            throw new InvalidOperationException("Cannot afford to add this shard");

        if (!IpAddresses.ContainsKey(shard.PublicIpAddress))
            throw new InvalidOperationException("Shard's IP is not available in this node");

        _shards.Add(shard.Id, shard);
    }

    /// <summary>
    /// Remove a shard from the node if it exists.
    /// </summary>
    /// <param name="shard">The shard to remove</param>
    public void RemoveShard(ShardId shardId) =>
        _shards.Remove(shardId);

    /// <summary>
    /// Add a dedicated IP address to the node.
    /// </summary>
    /// <param name="publicIp">The public IP</param>
    /// <param name="privateIp">The private IP to bind to</param>
    /// <exception cref="InvalidOperationException">Occurs if the IP cannot be added to the node</exception>
    public void AddDedicatedIpAddress(IPAddress publicIp, IPAddress privateIp)
    {
        if (IpAddresses.ContainsKey(publicIp))
            throw new InvalidOperationException("Public IP is already in use by this node");

        if (IpAddresses.Values.Contains(privateIp))
            throw new InvalidOperationException("Private IP is already in use by this node");

        _ipAddresses.Add(publicIp, privateIp);
    }

    /// <summary>
    /// Remove a dedicated IP address from the node if it exists.
    /// </summary>
    /// <param name="publicIp">The public IP to remove</param>
    /// <exception cref="InvalidOperationException">Occurs if the IP cannot be removed from the node</exception>
    public void RemoveDedicatedIpAddress(IPAddress publicIp)
    {
        if (publicIp == _primaryIpAddress)
            throw new InvalidOperationException("Cannot remove primary IP address from node");

        _ipAddresses.Remove(publicIp);
    }
}
