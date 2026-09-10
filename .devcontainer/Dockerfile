# Devcontainer for ORIGAM backend dev/debug on Linux (net8.0 subset only).

FROM mcr.microsoft.com/dotnet/sdk:8.0

# libgdiplus for System.Drawing (reporting).
RUN apt-get update \
 && apt-get -y install --no-install-recommends \
      ca-certificates curl sudo git make g++ xmlstarlet \
      libgif-dev autoconf libtool automake gettext \
      libglib2.0-dev libcairo2-dev libtiff-dev libexif-dev libpango1.0-dev \
 && git clone --depth 1 https://github.com/mono/libgdiplus.git /tmp/libgdiplus \
 && cd /tmp/libgdiplus && ./autogen.sh --with-pango --prefix=/usr && make && make install \
 && cd / && rm -rf /tmp/libgdiplus \
 && apt-get -y purge autoconf automake libtool \
 && apt-get -y autoremove && rm -rf /var/lib/apt/lists/*

# netcoredbg (Samsung, MIT) — OSS DAP debugger. Pinned to 3.1.3: 3.2.0+ needs
# glibc 2.38 but sdk:8.0 (Debian bookworm) has 2.36.
RUN ARCH=$(dpkg --print-architecture) \
 && curl -fsSL "https://github.com/Samsung/netcoredbg/releases/download/3.1.3-1062/netcoredbg-linux-${ARCH}.tar.gz" \
    | tar -xz -C /opt \
 && ln -s /opt/netcoredbg/netcoredbg /usr/local/bin/netcoredbg
