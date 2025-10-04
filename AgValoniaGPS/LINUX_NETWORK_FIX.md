# Linux Network Fix for AgValoniaGPS

## Problem
On Debian/Ubuntu, the app shows "Connected to 127.0.1.1" instead of the actual network IP. This prevents NTRIP from working across networks.

## Root Cause
The `GetLocalIPAddress()` method uses `Dns.GetHostEntry()` which returns the `/etc/hosts` entry (127.0.1.1) instead of actual network interfaces.

## Quick Fix (Without Code Changes)

### Option 1: Edit /etc/hosts (Temporary)
```bash
sudo nano /etc/hosts
# Comment out the 127.0.1.1 line:
# 127.0.1.1    your-hostname
```

### Option 2: Set Network Priority (Permanent)
```bash
# Check your network interfaces
ip addr show

# If your network is on eth0 or wlan0, ensure it's up
sudo ip link set eth0 up
```

## Code Fix (Permanent Solution)

Replace the `GetLocalIPAddress()` method in `UdpCommunicationService.cs`:

```csharp
private string? GetLocalIPAddress()
{
    try
    {
        // Use NetworkInterface for better Linux compatibility
        var networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
            .Where(ni => ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                         ni.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
            .ToList();

        foreach (var ni in networkInterfaces)
        {
            var properties = ni.GetIPProperties();
            var addresses = properties.UnicastAddresses
                .Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork)
                .ToList();

            foreach (var addr in addresses)
            {
                var ip = addr.Address.ToString();
                // Skip loopback and link-local addresses
                if (!ip.StartsWith("127.") && !ip.StartsWith("169.254."))
                {
                    return ip;
                }
            }
        }

        // Fallback to original method if no suitable interface found
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork &&
                !ip.ToString().StartsWith("127."))
            {
                return ip.ToString();
            }
        }
    }
    catch { }

    return "0.0.0.0"; // Return a valid IP instead of null
}
```

## Additional Network Issues

### For NTRIP across different networks:

1. **Check firewall**:
```bash
sudo ufw status
# If active, allow outgoing:
sudo ufw allow out 2101/tcp  # Common NTRIP port
```

2. **Check routing**:
```bash
# View routing table
ip route show
# Should show default route for internet access
```

3. **Test NTRIP connectivity**:
```bash
# Test if you can reach the NTRIP caster
nc -zv your-ntrip-server 2101
```

## Module Communication Fix

If modules are on a different subnet (192.168.5.x), you may need to:

1. **Add a route**:
```bash
# Add route to module subnet (adjust interface name)
sudo ip route add 192.168.5.0/24 dev eth0
```

2. **Or use multiple network interfaces**:
- eth0 for internet/NTRIP
- eth1 or USB adapter for module network

## Testing

After applying fixes:
1. Restart AgValoniaGPS
2. Check Data I/O - should show actual IP (e.g., 192.168.1.100)
3. NTRIP should now connect across networks
4. Modules should still communicate on their subnet