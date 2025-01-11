using DataManager.Core.Commands.Contracts;

namespace DataManager.Core.Commands.DefaultCommands;
public class GetInfoCommand : ICommand
{
    public void Execute()
    {
        Console.WriteLine("\nTo retrieve your Instagram information (JSON or HTML files):");
        Console.WriteLine("\t1. Open the Instagram app or website.");
        Console.WriteLine("\t2. Navigate to your profile by tapping on your profile picture in the bottom-right corner.");
        Console.WriteLine("\t3. Tap on the menu icon (three horizontal lines) in the top-right corner.");
        Console.WriteLine("\t4. Go to 'Settings and privacy'.");
        Console.WriteLine("\t5. Scroll down and select 'Your information and permissions'.");
        Console.WriteLine("\t5a. Tap on 'Download your information'.");
        Console.WriteLine("\t6. Select 'Download or transfer your information'.");
        Console.WriteLine("\t   - Note: If you want to work with past requests, you may see previously requested data here.");
        Console.WriteLine("\t7. Under 'Download or transfer your information,' you’ll see options for 'All available information' and 'Some of your information'.");
        Console.WriteLine("\t   - Click 'Some of your information'.");
        Console.WriteLine("\t8. From the 'Connections' tab, choose only 'Followers' and 'Following'.");
        Console.WriteLine("\t9. Tap 'Next' to proceed.");
        Console.WriteLine("\t10. Select 'Download to device'.");
        Console.WriteLine("\t11. Set the date range to 'All time'.");
        Console.WriteLine("\t12. Choose the format as 'JSON'.");
        Console.WriteLine("\t13. Media quality does not matter, but 'Low' is recommended for faster processing.");
        Console.WriteLine("\t14. Confirm and submit the request.");
        Console.WriteLine("\t15. You’ll receive an email with a link to download the data. Follow the link to save the file.\n");
    }
}
