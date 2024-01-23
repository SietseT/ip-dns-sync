# ip-dns-sync
Keeps your (public) DNS records up-to-date whenever you have a dynamic IP address. 
Never lose external access again when your ISP decides to change your external IP address.

---

- [Features](#features)
- [Usage](#usage)
- [Configuration](#configuration)
- [Providers](#providers)
    - [TransIP](#transip)

---

## Features
- Automatically checks if your external IP address has changed and updates specified A records for your DNS provider
- Support for multiple DNS providers
  Currently only TransIP though, but it is easy to add other providers
- Cross-platform support - runs as Docker container

---

## Usage
The Docker image of `ip-dns-sync` is published to Docker Hub and can either be run as a standalone Docker command, or via Docker Compose.

Docker run:
```bash
docker run sietsetro/ip-dns-sync:latest
```

The `examples` directory contains an example of a Docker Compose file:

```yaml
services:
  ip-dns-sync:
    container_name: ip-dns-sync
    image: sietsetro/ip-dns-sync:latest
    restart: always
```

Note that the above examples do **not** have any DNS providers configured.
Starting the container this way will result in an error during startup.

---

## Configuration
Several settings can be configured to modify the behavior the tool. These should be set as environment variables.

| Environment variable | Purpose | Default |
| -------------------- | ------- | --------|
| Settings__UpdateIntervalInMinutes | Time between checks | 5 |

For configuring providers, please refer to [providers](#providers).

---

## Providers
This section describes how to configure each DNS provider.

### TransIP
To be able to access the TransIP API using your account, you need to create a Key Pair in the TransIP control panel:
1. Go to https://www.transip.nl/cp/account/api/
1. Under _Key Pairs_:
    1. Fill in the _Label_ (e.g. `ip-dns-sync`) field
    1. Tick or untick the checkbox to limit access to the TransIP API only from specified IP addresses.
       This is up to you.
    1. Add the Key Pair.
1. The private key is now visible on the page. 
   **This is the only time it is visible. So copy it now and store it somewhere safe.**

Now that you have a private key, you can add the necessary environment variables to configure the TransIP provider:

| Environment variable | Purpose |
| -------------------- | ------- |
| Providers__TransIP__Username | Your TransIP username |
| Providers__TransIP__PrivateKey | The private key that you created earlier |
| Providers__TransIP__Domains__0 | The domain(s) you would like to keep in sync with your external IP address. This can be either a rootdomain or subdomain (example.com or sub.example.com). To specify multiple domains, simply add a new environment variable and increment the number at the end. For example:  `Providers__TransIP__Domains__1` |

The code below shows a fully configurated Docker Compose file with the TransIP provider configured:

```yaml
services:
  ip-dns-sync:
    container_name: ip-dns-sync
    image: sietsetro/ip-dns-sync:latest
    environment:
      Providers__TransIP__Domains__0: example.com
      Providers__TransIP__Domains__1: sub.example.com
      Providers__TransIP__Username: ExampleUser
      Providers__TransIP__PrivateKey: |
        -----BEGIN PRIVATE KEY-----
        Your very secret private key
        -----END PRIVATE KEY-----
    restart: always
```