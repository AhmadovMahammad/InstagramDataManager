# Instagram Automation Tool

## Overview

The Instagram Automation Tool is a framework designed to automate various Instagram-related tasks using both file processing and web automation (via Selenium WebDriver). It supports operations such as displaying follow requests, managing blocked profiles, and automating interactions with Instagram directly, like unliking posts.

## Persistent Login
The tool supports persistent login. Users only need to log in once during the program's lifecycle. This avoids repeated sign-ins for each Selenium-based task, improving user experience and execution efficiency.

## Supported Operations
The tool supports the following operations:

![image](https://github.com/user-attachments/assets/494434af-11b5-4f30-a48b-61fca1f5a02f)


## Browser Support for Selenium-based Operations

For Selenium-based operations like unliking all posts, the tool is designed to work with Firefox Developer Edition. The tool will automatically attempt to locate the Firefox executable. If it is not found, the user will be prompted to manually provide the path to the executable.

```
Please enter the path to your Firefox Developer Edition executable \n(e.g., C:\\Program Files\\Firefox Developer Edition\\firefox.exe):
```

## Problem and Solution

### Problem:
Instagram tasks require handling both static data (file-based operations) and dynamic interactions (via web automation). Challenges include ensuring smooth interaction between these two modules, managing login sessions, and avoiding repetitive logins for Selenium-based tasks.

### Solution:
- Persistent Login: Users log in once at the start of the program, and the session is maintained throughout the program lifecycle.
- Settings XML: Allows users to configure browser behavior, including enabling headless mode and bot detection bypassing.
- File Format Strategies: Unified approach to process multiple file formats (JSON, HTML) with minimal user input.
- Browser Detection: Automated handling of Firefox Developer Edition setup to simplify configuration.

## Usage

### 1. Configure Settings
Before running the tool, users can adjust settings in the settings.xml file to customize the browser behavior.

### 2. Run the Application:
Run the tool and log in to Instagram at the start of the program. The login session will persist for subsequent tasks.

### 3. Select an Operation:
When prompted, enter the operation number corresponding to the task you want to perform. For example:

```
Command (Enter a number for command or type 'exit' to quit)
> 9
```

### File-based Operations:
If you select a file-based operation (e.g., operation 2), you will be asked to provide the file path:

```
Command (Enter a number for command or type 'exit' to quit)
> 2
Enter the file path to proceed
> C:/Users/username/Documents/instagram_data.json
```

After entering the file path, the operation will proceed and the relevant data will be processed and displayed.

## Contributing

Contributions are welcome! If you have an idea for a new feature, bug fix, or improvement, feel free to fork the repository and submit a pull request.

## Support
If you find this tool helpful, you can support its development by buying me a coffee: [buymeacoffee.com](https://www.buymeacoffee.com/ahmadovmahammad)
