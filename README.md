# Instagram Automation Tool

## Overview

The Instagram Automation Tool is a framework designed to automate various Instagram-related tasks using both file processing and web automation (via Selenium WebDriver). It supports different operations, from displaying follow requests and blocked profiles using file-based data to interacting with Instagram directly through Selenium for tasks like unliking posts.


## Supported Operations

The tool supports the following operations:

- **Display Recent Follow Requests**
- **Display Received Requests**
- **...**
- **Unlike All Posts** (Selenium-based)

![image](https://github.com/user-attachments/assets/83112ae0-09f2-4111-b9d4-f1103dec369a)


## Browser Support for Selenium-based Operations

For Selenium-based operations like unliking all posts, the tool is designed to work with Firefox Developer Edition. The tool will attempt to locate the Firefox executable automatically. 
If it's not found, the user will be prompted to enter the path to the Firefox executable.


## Libraries Used

The tool uses the following libraries:

- **Selenium WebDriver** – for automating browser interactions with Instagram.
- **Selenium.Support** – for supporting Selenium operations.
- **ConsoleTables** – for creating formatted console tables for displaying data.


## Problem and Solution

### Problem:
The goal was to create an Instagram automation tool that could handle multiple types of tasks, including both file-based tasks and Selenium-based web automation. 
The major challenge was ensuring smooth interaction between file processing and web automation, as well as handling dynamic scenarios like interacting with the Instagram login page via Selenium.

### Solution:

- Developed a file-based operation framework to process data from files in multiple formats (JSON, HTML).
- Integrated Selenium WebDriver to automate actions like logging into Instagram, unliking posts.
- Implemented file format strategies to handle various file types, allowing users to process data in different formats easily.
- Incorporated browser detection and handling for Firefox Developer Edition, providing an automatic setup for the user.

## Usage

### 1. Run the Application:
Execute the application to begin the interaction.

### 2. Select an Operation:
When prompted, enter the operation number corresponding to the task you want to perform. For example:

```
Operation (Enter a number for an operation or type 'exit' to quit): 2
Enter the file path to proceed:
```

- **Operations 1-8**: File-based operations that require the user to provide a file path.
- **Operations 9**: Selenium-based operations that will open Firefox Developer Edition and require login credentials.

### File-based Operations:
If you select a file-based operation (e.g., operation 2), you will be asked to provide the file path:

```
Operation (Enter a number for an operation or type 'exit' to quit): 2
Enter the file path to proceed:
```

After entering the file path, the operation will proceed and the relevant data will be processed and displayed.

### Selenium-based Operations:
For Selenium-based operations (like operation 9), the application will launch Firefox Developer Edition and prompt you for your Instagram credentials:

```
Operation (Enter a number for an operation or type 'exit' to quit): 9
ID   geckodriver     INFO    Listening on 127.0.0.1:PORT
ID   mozrunner::runner       INFO    Running command: "C:\\Program Files\\Firefox Developer Edition\\firefox.exe" "--marionette" "-no-remote" "-profile" "C:\\Users\\...\\AppData\\Local\\Temp\\...."
Read port: port number
Instagram login page opened successfully!
Enter username: username
Enter password: **********
```

If Firefox Developer Edition is not installed or the path is not found, the tool will prompt you to provide the path to the Firefox executable:

```
Please enter the path to your Firefox Developer Edition executable \n(e.g., C:\\Program Files\\Firefox Developer Edition\\firefox.exe): 
```

## Contributing

Contributions are welcome! If you have an idea for a new feature, bug fix, or improvement, feel free to fork the repository and submit a pull request.
