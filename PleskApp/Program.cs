using Renci.SshNet;
using System;

class Program
{
    static void Main(string[] args)
    {
        string host = "92.113.25.70";
        string username = "root";
        string password = "Password@123";

        using (var client = new SshClient(host, username, password))
        {
            client.Connect();
            var subdomain = "yousef";

            try
            {
                // Create the subdomain
                string createSubdomainCommand = $"sudo plesk bin subdomain --create {subdomain} -domain sabilwater.com -www-root {subdomain}";
                var createSubdomainResult = client.RunCommand(createSubdomainCommand);
                Console.WriteLine("Subdomain Creation Result: " + createSubdomainResult.Result);
                Thread.Sleep(2000);

                // Configure SSL
                string sslCommand = $"sudo plesk bin extension --exec letsencrypt cli.php --domain {subdomain}.sabilwater.com --email newin386@gmail.com --agree-tos --letsencrypt-ssl";
                var sslResult = client.RunCommand(sslCommand);
                Console.WriteLine("SSL Configuration Result: " + sslResult.Result);
                Thread.Sleep(2000);
                // Update Nginx Configuration
                string nginxConfig = $"/etc/nginx/plesk.conf.d/vhosts/{subdomain}.sabilwater.com.conf";
                string command = @"sed -i '/location \//,/}/c\location / {\n    proxy_pass http://localhost:3000;\n    proxy_hide_header upgrade;\n    proxy_set_header Host             \$host;\n    proxy_set_header X-Real-IP        \$remote_addr;\n    proxy_set_header X-Forwarded-For  \$proxy_add_x_forwarded_for;\n    proxy_set_header X-Accel-Internal /internal-nginx-static-location;\n    access_log off;\n}' "+ nginxConfig;
                Thread.Sleep(2000);


                var updateNginxConfigResult = client.RunCommand(command);
                Console.WriteLine("Nginx Config Update Result: " + updateNginxConfigResult.Result);

                // Restart Nginx
                var restartNginxResult = client.RunCommand("sudo systemctl restart nginx");
                Console.WriteLine("Nginx Restart Result: " + restartNginxResult.Result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            finally
            {
                client.Disconnect();
            }
        }
    }
}
