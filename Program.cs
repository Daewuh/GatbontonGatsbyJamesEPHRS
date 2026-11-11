using System.Globalization;

namespace EPHRS;

public class Program
{
    // Starts EMPTY (0 pre-defined records), as required.
    private static readonly List<Employee> employees = new();
    private static int nextId = 1001;

    // >>> MERGE CONFLICT LINE: change this line differently on main and on your branch <<<
    private const string AppTitle = "EMPLOYEE PAYROLL & HR MANAGEMENT SYSTEM";

    public static void Main()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("==========================================");
            Console.WriteLine($" {AppTitle}");
            Console.WriteLine("==========================================");
            Console.WriteLine(" 1. Create employee");
            Console.WriteLine(" 2. Read (list all employees)");
            Console.WriteLine(" 3. Search / Filter");
            Console.WriteLine(" 4. Update employee");
            Console.WriteLine(" 5. Delete employee");
            Console.WriteLine(" 0. Exit");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": CreateEmployee(); break;
                case "2": ReadEmployees(); break;
                case "3": SearchMenu(); break;
                case "4": UpdateEmployee(); break;
                case "5": DeleteEmployee(); break;
                case "0": Console.WriteLine("Goodbye!"); return;
                default: Console.WriteLine("Invalid option."); break;
            }
        }
    }

    // ---------------------------------------------------------------- CREATE
    private static void CreateEmployee()
    {
        Console.WriteLine("\n--- Create Employee ---");
        var emp = new Employee
        {
            Id = nextId,
            FirstName = ReadString("First name: "),
            LastName = ReadString("Last name: "),
            Department = ReadString("Department (IT, HR, Sales, Finance...): "),
            JobTitle = ReadString("Job title: "),
            MonthlySalary = ReadDecimal("Monthly salary: "),
            TaxRate = ReadTaxRate("Tax rate (e.g. 0.12 or 12): "),
            HireDate = ReadDate("Hire date (yyyy-MM-dd): "),
            IsActive = ReadBool("Active? (y/n): "),
            Email = ReadString("Email: ")
        };
        employees.Add(emp);
        nextId++;
        Console.WriteLine($"Employee created with Id {emp.Id}.");
    }

    // ------------------------------------------------------------------ READ
    private static void ReadEmployees()
    {
        Console.WriteLine("\n--- All Employees ---");
        PrintTable(employees.OrderBy(e => e.Id));
    }

    // ---------------------------------------------------------- SEARCH/FILTER
    private static void SearchMenu()
    {
        Console.WriteLine("\n--- Search / Filter ---");
        Console.WriteLine(" 1. By keyword (name, title, email)");
        Console.WriteLine(" 2. By department");
        Console.WriteLine(" 3. By status (active / inactive)");
        Console.WriteLine(" 4. By salary range");
        Console.WriteLine(" 5. By Id");
        Console.Write("Choose: ");

        IEnumerable<Employee> result;
        switch (Console.ReadLine()?.Trim())
        {
            case "1":
                var kw = ReadString("Keyword: ");
                result = employees.Where(e =>
                    e.FirstName.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    e.LastName.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    e.JobTitle.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    e.Email.Contains(kw, StringComparison.OrdinalIgnoreCase));
                break;
            case "2":
                var dept = ReadString("Department: ");
                result = employees.Where(e => e.Department.Equals(dept, StringComparison.OrdinalIgnoreCase));
                break;
            case "3":
                bool active = ReadBool("Show active employees? (y = active, n = inactive): ");
                result = employees.Where(e => e.IsActive == active);
                break;
            case "4":
                decimal min = ReadDecimal("Minimum salary: ");
                decimal max = ReadDecimal("Maximum salary: ");
                result = employees.Where(e => e.MonthlySalary >= min && e.MonthlySalary <= max);
                break;
            case "5":
                int id = ReadInt("Id: ");
                result = employees.Where(e => e.Id == id);
                break;
            default:
                Console.WriteLine("Invalid option.");
                return;
        }
        PrintTable(result.OrderBy(e => e.LastName).ThenBy(e => e.FirstName));
    }

    // ---------------------------------------------------------------- UPDATE
    private static void UpdateEmployee()
    {
        Console.WriteLine("\n--- Update Employee ---");
        int id = ReadInt("Id of employee to update: ");
        var emp = employees.FirstOrDefault(e => e.Id == id);
        if (emp == null) { Console.WriteLine("Employee not found."); return; }

        Console.WriteLine("Press Enter to keep the current value.");
        emp.FirstName = ReadOptional($"First name [{emp.FirstName}]: ", emp.FirstName);
        emp.LastName = ReadOptional($"Last name [{emp.LastName}]: ", emp.LastName);
        emp.Department = ReadOptional($"Department [{emp.Department}]: ", emp.Department);
        emp.JobTitle = ReadOptional($"Job title [{emp.JobTitle}]: ", emp.JobTitle);

        var sal = ReadOptional($"Monthly salary [{emp.MonthlySalary}]: ", "");
        if (decimal.TryParse(sal, NumberStyles.Number, CultureInfo.InvariantCulture, out var s) && s >= 0)
            emp.MonthlySalary = s;

        var tax = ReadOptional($"Tax rate [{emp.TaxRate}]: ", "");
        if (double.TryParse(tax, NumberStyles.Float, CultureInfo.InvariantCulture, out var t) && t >= 0)
            emp.TaxRate = t > 1 ? t / 100 : t;

        var hd = ReadOptional($"Hire date [{emp.HireDate:yyyy-MM-dd}]: ", "");
        if (DateTime.TryParse(hd, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            emp.HireDate = d;

        var act = ReadOptional($"Active? (y/n) [{(emp.IsActive ? "y" : "n")}]: ", "").ToLower();
        if (act is "y" or "yes") emp.IsActive = true;
        else if (act is "n" or "no") emp.IsActive = false;

        emp.Email = ReadOptional($"Email [{emp.Email}]: ", emp.Email);
        Console.WriteLine("Employee updated.");
    }

    // ---------------------------------------------------------------- DELETE
    private static void DeleteEmployee()
    {
        Console.WriteLine("\n--- Delete Employee ---");
        int id = ReadInt("Id of employee to delete: ");
        var emp = employees.FirstOrDefault(e => e.Id == id);
        if (emp == null) { Console.WriteLine("Employee not found."); return; }

        if (ReadBool($"Delete {emp.FirstName} {emp.LastName}? (y/n): "))
        {
            employees.RemoveAll(e => e.Id == id);
            Console.WriteLine("Employee deleted.");
        }
        else Console.WriteLine("Delete cancelled.");
    }

    // --------------------------------------------------------------- DISPLAY
    private static void PrintTable(IEnumerable<Employee> rows)
    {
        var list = rows.ToList();
        if (list.Count == 0) { Console.WriteLine("No records found."); return; }

        Console.WriteLine(
            $"{"Id",-5} {"First",-10} {"Last",-10} {"Dept",-9} {"Title",-16} {"Salary",11} {"Tax",5} {"Hired",-10} {"Active",-6} {"Net Pay",11} Email");
        Console.WriteLine(new string('-', 130));
        foreach (var e in list)
        {
            Console.WriteLine(
                $"{e.Id,-5} {e.FirstName,-10} {e.LastName,-10} {e.Department,-9} {e.JobTitle,-16} " +
                $"{e.MonthlySalary,11:N2} {e.TaxRate,5:P0} {e.HireDate,-10:yyyy-MM-dd} {(e.IsActive ? "Yes" : "No"),-6} " +
                $"{e.NetSalary,11:N2} {e.Email}");
        }
        Console.WriteLine(new string('-', 130));
        Console.WriteLine($"Records: {list.Count} | Total gross: {list.Sum(x => x.MonthlySalary):N2} | " +
                          $"Total net: {list.Sum(x => x.NetSalary):N2} | Active: {list.Count(x => x.IsActive)}");
    }

    // --------------------------------------------------------- INPUT HELPERS
    private static string ReadString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(s)) return s;
            Console.WriteLine("Value cannot be empty.");
        }
    }

    private static string ReadOptional(string prompt, string current)
    {
        Console.Write(prompt);
        var s = Console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(s) ? current : s;
    }

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out var v)) return v;
            Console.WriteLine("Enter a valid whole number.");
        }
    }

    private static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (decimal.TryParse(Console.ReadLine(), NumberStyles.Number, CultureInfo.InvariantCulture, out var v) && v >= 0)
                return v;
            Console.WriteLine("Enter a valid non-negative amount.");
        }
    }

    private static double ReadTaxRate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (double.TryParse(Console.ReadLine(), NumberStyles.Float, CultureInfo.InvariantCulture, out var v) && v >= 0 && v <= 100)
                return v > 1 ? v / 100 : v;   // accepts 12 or 0.12
            Console.WriteLine("Enter a rate such as 0.12 or 12.");
        }
    }

    private static DateTime ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (DateTime.TryParse(Console.ReadLine(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var v)) return v;
            Console.WriteLine("Enter a valid date (yyyy-MM-dd).");
        }
    }

    private static bool ReadBool(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = Console.ReadLine()?.Trim().ToLower();
            if (s is "y" or "yes") return true;
            if (s is "n" or "no") return false;
            Console.WriteLine("Enter y or n.");
        }
    }
}
