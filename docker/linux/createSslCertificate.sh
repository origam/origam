#!/bin/bash

file_exists() {
    [ -e "$1" ]
}

if file_exists "server.key" || file_exists "server.csr" || file_exists "server.crt"; then
    echo "SSL certificate files already exist. No files will be created."
else
    error_occurred=false

    # Generate the private key
    if ! openssl genrsa -out server.key 2048 > /dev/null 2>&1; then
        echo "Error generating private key"
        error_occurred=true
    fi

    # Create a CSR with default values
    if ! openssl req -new -key server.key -out server.csr -subj "/C=US/ST=State/L=City/O=Organization/OU=OrganizationalUnit/CN=example.com" > /dev/null 2>&1; then
        echo "Error creating CSR"
        error_occurred=true
    fi

    # Generate the self-signed certificate
    if ! openssl x509 -req -days 365 -in server.csr -signkey server.key -out server.crt > /dev/null 2>&1; then
        echo "Error generating self-signed certificate"
        error_occurred=true
    fi

    if [ "$error_occurred" = false ]; then
        echo "Development self-signed SSL certificate files created."
    fi
fi