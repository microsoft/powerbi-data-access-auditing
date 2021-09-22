
set "curpath=%cd%"
set "mapping=%curpath%:/usr/local/structurizr"
echo "Run the command below:"
echo "docker run -v %mapping% -d -p 8080:8080 structurizr/lite"