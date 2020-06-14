cmake_minimum_required(VERSION 2.8.9)
project (fswebcamtest)

add_executable(${PROJECT_NAME} fswebcamtest.cpp)

target_link_libraries(${PROJECT_NAME} EltraFsWebCam)
