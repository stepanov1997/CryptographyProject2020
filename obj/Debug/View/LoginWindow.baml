   M S B A M L   `   `   `  ����R  NCryptographyProject2019, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullA%clr-namespace:CryptographyProject2019CryptographyProject2019  5   	   .    (CryptographyProject2019.View.LoginWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   7local%clr-namespace:CryptographyProject2019     5   	     � Title$  LoginWindow��5   	   $	��431��6   $��503.333��6*    � 
ResizeMode$ NoResize=�6:    � WindowStartupLocation$ CenterScreen=�6P   .��5	      � $��0,0,-1.333,-4.667q�6   ��5
   
   �� 5       �� ImageSource$ /Resources/background.jpg��6   .+�5   
   �� $��LOGIN ON THE SYSTEM��6   $
��Left=�6.   $��
155,40,0,0q�6I   $	��Top=�6]   $��30��5      $	��185��6   $��16(�6(   $
��Bold"�66   $��Italic$�6H    J� RenderTransformOrigin$ 0.596,-0.156&�5      
�������65   �� 5   
   $��	Username:��6   $
��Left=�6$   $��105,115,0,0q�6?   $	��Top=�6T   $��30��6l   $	��105��5      
�������6   �� 5   
   -   5        $��UsernameTextBox��5      $
��Left=�6)   $��23��6D   $��235,120,0,0q�6P    �� TextWrapping$
 Wrap=�6e   $	��Top=�5      $	��140��6*    � ToolTip$ Type username��66   �� 5   
   $��	Password:��6   $
��Left=�6$   $��105,165,0,0q�6?   $	��Top=�6T   $��30��6l   $	��160��5      
�������6   G� 5   
   -   5        $��PasswordBox��5      $
��Left=�6)   $��23��6D   $��235,170,0,0q�6P   $	��Top=�5      $	��140��6.   $ Type password��6:   �� 5   
   $��Path to certificate:��6   $
��Left=�6/   $��105,217,0,0q�6J   $	��Top=�6_   $��30��5      $	��110��6   
�������6(   �� 5   
   -   5        $��PathTextBox��5      $ NoWrap=�6%   $
��Left=�6;   $��23��6V   $��235,217,0,0q�6b   $	��Top=�5      $	��140��6*   $ Type absolute path��66   5      �� 5   
   -   5        $��LoginButton��5      $��LOG IN��6$   $
��Left=�65   $��200,285,0,0q�6P   $	��Top=�5      $	��100��6)   $ 0.538,2&�65   6U   �� 5   
   -   5        $��REGISTER��5      $
��Left=�6$   $��200,314,0,0q�6?   $	��Top=�6T   $	��100��6l   5       �� 5!   
   -   5        $��Make user certificate��5!   .   $
��Left=�6N   $��185,345,0,0q�5"      $	��Top=�6&   $	��130��6>   $��20��6J   5#      5$      