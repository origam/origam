cls
cd ..
docker stop origam_local
docker rm origam_local
docker rmi origam_local_image:1.0
docker build -t origam_local_image:1.0 -f DockerfileServer.linux .
docker run --env-file "<path-to-origam-repo>\origam\docker\debug\model-test_Linux.env" ^
    -it --name origam_local ^
    -v "<path-to-origam-repo>\origam\model-tests\model":/home/origam/projectData/model ^
    -p 443:443 ^
    origam_local_image:1.0
    
    
    
    
    

