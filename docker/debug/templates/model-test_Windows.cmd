docker run --env-file "<path-to-origam-repo>\origam\docker\debug\model-test_Windows.env" ^
    -it --name origam-2025.9.0.3999 ^
    -v "<path-to-origam-repo>\origam\model-tests\model":C:\home\origam\projectData\model ^
    -p 443:443 ^
    origam/server:2025.9.0.3999.win-core

