# Devcontainer for ORIGAM full-stack dev/debug on Linux (net10.0 subset only).

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble

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

# netcoredbg (Samsung, MIT) — OSS DAP debugger.
RUN ARCH=$(dpkg --print-architecture) \
 && curl -fsSL "https://github.com/Samsung/netcoredbg/releases/download/3.2.0-1092/netcoredbg-linux-${ARCH}.tar.gz" \
    | tar -xz -C /opt \
 && ln -s /opt/netcoredbg/netcoredbg /usr/local/bin/netcoredbg

# Node 22 + corepack (yarn) so the Vite dev servers of all three frontends run
# here on demand and the IDE gets TypeScript types from node_modules volumes.
RUN curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
 && apt-get update \
 && apt-get -y install --no-install-recommends nodejs jq ripgrep vim \
 && rm -rf /var/lib/apt/lists/* \
 && corepack enable
# Corepack prompts before downloading the packageManager-pinned version; that
# hangs non-interactive contexts (postCreateCommand, tasks), so silence it.
ENV COREPACK_ENABLE_DOWNLOAD_PROMPT=0

WORKDIR /workspaces/origam
