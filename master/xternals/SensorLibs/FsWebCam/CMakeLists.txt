cmake_minimum_required(VERSION 2.8.12)
project (EltraFsWebCam)

if (NOT CMAKE_BUILD_TYPE) 
	set (CMAKE_BUILD_TYPE Release)
endif ()

if (WIN32)
	include_directories(C:/opencv/build/include)
	link_directories(C:/opencv/src/opencv/build/lib/Debug)
else()
  # set stuff for other systems
endif()

set(DHT22_PATH ${CMAKE_SOURCE_DIR}/)

add_library(${PROJECT_NAME} SHARED ${DHT22_PATH}/fs_web_cam.cpp 
								   ${DHT22_PATH}/sys_helper.cpp)

target_link_libraries(${PROJECT_NAME} opencv_videoio430d opencv_video430d opencv_core430d opencv_highgui430d opencv_photo430d opencv_imgproc430d opencv_ml430d opencv_objdetect430d opencv_stitching430d opencv_calib3d430d opencv_dnn430d opencv_features2d430d opencv_flann430d opencv_gapi430d opencv_imgcodecs430d)

install(TARGETS ${PROJECT_NAME} LIBRARY DESTINATION /usr/lib)