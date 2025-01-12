# Instagram Automation Tool

## Overview

The **Instagram Automation Tool** is a framework designed to automate various Instagram-related tasks, combining both file processing and web automation using **Selenium WebDriver**. It supports operations such as:

- Displaying follow requests
- Managing blocked profiles
- Automating interactions with Instagram (e.g., unliking posts)

## Persistent Login

The tool supports **persistent login**, improving user experience and execution efficiency. Users log in once, and the session is maintained for the duration of the program's lifecycle, avoiding repeated sign-ins for each Selenium-based task.

- **Credentials Security**: Saved credentials are encrypted using the **Windows Data Protection API**, ensuring they are **machine-specific** and secure.

## Login Workflow

### 1. Retrieve Existing Credentials
- The tool checks for previously saved credentials.
- Users can choose to reuse the saved credentials or overwrite them with new ones.

### 2. Credential Storage
- Credentials are saved in a specified file path.
- Passwords are encrypted and can only be decrypted on the same machine.

## Supported Operations

![Supported Operations](https://github.com/user-attachments/assets/3b743953-eaf4-4777-8790-eea5eb47fc3c)

## Detailed Features

### 1. Manage Followers
- **Workflow**
  1. Users input the username of the profile to process.
  2. The tool verifies access and checks if the profile is being followed.
  3. Followers and following data are extracted.
  4. The tool compares current data with previously saved data:
     - Identify new followers/following
     - Detect users who unfollowed or were removed
  5. Current data can be saved for future comparisons.

### 2. Unlike All Posts
- **Workflow**
  1. The tool navigates to the activity section of the user's profile.
  2. It systematically unlikes posts until no more posts exist or spam detection occurs.
  3. The tool supports unliking **up to 500-600 posts** per session.

### 3. Manage Recent and Pending Follow Requests
- **Workflow**
  1. Users upload a JSON or HTML file containing request data.
  2. The tool identifies all pending requests from the uploaded file.
  3. Users can decline all requests with a single command.

### 4. Manage Received Follow Requests
- **Workflow**
  1. Users can confirm or delete all received requests at once.
  2. Actions are performed automatically based on user input.

### 5. Other Operations

- **Manage Blocked Profiles**: View and manage blocked accounts.
- **Manage Close Friends**: Update your close friends list efficiently.

## Problem and Solution

### Problem
Instagram tasks require handling both static data (file-based operations) and dynamic interactions (via web automation). The challenges include ensuring smooth interaction between these two modules, managing login sessions, and avoiding repetitive logins for Selenium-based tasks.

### Solution
- **Persistent Login**: Log in once at the start of the program, and the session is maintained throughout the lifecycle.
- **Settings XML**: Customize browser behavior, including enabling headless mode and bypassing bot detection.
- **File Format Strategies**: Unified approach to process multiple file formats (JSON, HTML) with minimal user input.
- **Browser Detection**: Automated handling of Firefox Developer Edition setup to simplify configuration.

## Usage

### 1. Configure Settings
Adjust the `settings.xml` file to customize browser behavior, such as enabling headless mode and bypassing bot detection.

### 2. Run the Application
Run the tool and log in to Instagram. The login session will persist for subsequent tasks.

### 3. Select an Operation
When prompted, enter the operation number corresponding to the task you want to perform. For example:

```
Command (Enter a number for command or type 'exit' to quit) > 9
```

### File-based Operations:
If you select a file-based operation (e.g., operation 8), you will be asked to provide the file path:

```
Command (Enter a number for command or type 'exit' to quit) > 8
Enter the file path to proceed > C:/Users/username/Documents/instagram_data.json
```

After entering the file path, the operation will proceed and the relevant data will be processed and displayed.

## Contributing

Contributions are welcome! If you have an idea for a new feature, bug fix, or improvement, feel free to fork the repository and submit a pull request.

## Support
If you find this tool helpful, you can support its development by buying me a coffee: [buymeacoffee.com](https://www.buymeacoffee.com/ahmadovmahammad)
