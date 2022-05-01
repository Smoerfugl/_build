dotnet pack -o output
FILE=$(find . -name "*.nupkg" -type f -printf '%h\n')

echo $FILE
dotnet tool update --global --add-source $FILE Smoerfugl._build
