#!/bin/sh
set -eu

dotnet tool restore
dotnet husky install
