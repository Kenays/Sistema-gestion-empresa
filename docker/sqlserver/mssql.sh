#!/usr/bin/env bash

read -p "Usuario: " USER
read -s -p "Contraseña: " PASS
echo

docker run -it --rm --network sqlserver_default mcr.microsoft.com/mssql-tools \
  /opt/mssql-tools/bin/sqlcmd -S empresa-sql,1433 \
                              -U $USER -P $PASS \
                              -w 140 -y 0 -Y 0

