   M S B A M L   `   `   `  ���� R  NCryptographyProject2019, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:CryptographyProject2019.ViewCryptographyProject2019.View  7    1CryptographyProject2019.View.FileEncryptionWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     <local*clr-namespace:CryptographyProject2019.View       � Title$  FileEncryptionWindow��$	��431��$��503.333�� � 
ResizeMode$ NoResize=� � WindowStartupLocation$ CenterScreen=�.��� $��0,0,-1.333,-4.667q�����  �� ImageSource$ /Resources/background.jpg��.+��� $��FILE ENCRYPTION��$
��Left=�$��
170,35,0,0q�$	��Top=�$��30��$	��185��$��16(�$
��Bold"�$��Italic$� J� RenderTransformOrigin$ 0.596,-0.156&�
��������� -   $
��Back��$
��Left=�$��
55,345,0,0q�$	��Top=�$��75���� $��File to encrypt:��
�������$
��Left=�$��
37,120,0,0q�$	��Top=��� -   $��
FileButton��$��Select��$
��Left=�$	��Top=�$	��160��$��175,120,0,0q�$��27���� $��Symmetric algorithm:��
�������$
��Left=�$��
37,165,0,0q�$	��Top=��� -   $��SymmetricComboBox��$
��Left=�$��175,165,0,0q�$	��Top=�$	��160��$��27�� �� SelectedValuePath$	 Tag�� �� SelectedValue �� *YourIntProperty+ �� Mode$ TwoWay=�.@��� -   $��AesItem��$	��AES��
 � Tag$ 0���� -   $��Rc4Item��$	��RC2��$ 1���� -   $��DesItem��$
��DES3��$ 2���� $��Hash algorithm��
�������$
��Left=�$	��Top=�$��
38,210,0,0q��� -   $��HashComboBox��$
��Left=�$��175,210,0,0q�$	��Top=�$	��160��$��27��$	 Tag�� �� *YourIntProperty+$ TwoWay=�.@��� -   $��SHA1Item��$
��SHA1��$ 0���� -	   $��
SHA256Item��$��SHA256��$ 1���� -
   $��EncryptButton��$��ENCRYPT FILE��$
��Left=�$��200,345,0,0q�$	��Top=�$	��115���� $��Username of receiver��
�������$
��Left=�$��
40,260,0,0q�$	��Top=��� -   $��ReceiverComboBox��$
��Left=�$��175,258,0,0q�$	��Top=�$	��160��$��27��