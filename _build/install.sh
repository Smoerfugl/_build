dotnet pack -o output
FILE=$(find . -name "*.nupkg" -type f -printf '%h\n')
dotnet tool update --global --add-source $FILE Smoerfugl._build
