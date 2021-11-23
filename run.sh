#! /bin/bash

rm -rf output
dotnet publish -o output -r linux-x64
sudo ./output/Harbour serve -d
sudo ps aux | grep -i harbour

sudo ./output/Harbour serve stop
sudo ps aux | grep -i harbour