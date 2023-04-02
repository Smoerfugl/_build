dotnet pack -o output
FILE=$(find . -name "*.nupkg" -type f -printf '%h\n')
dotnet tool uninstall --global Smoerfugl._build
dotnet tool install --global --add-source $FILE Smoerfugl._build
