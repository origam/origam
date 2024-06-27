import sys
import traceback


def ask_for_index(project_names):
    for i in range(len(project_names)):
        print(f"{i} {project_names[i]}")
    print("")
    return input("Choose index:")


def select_option(options, default=None):
    while True:
        index_str = ask_for_index(options)
        if index_str == '' and default is not None:
            return options[default]
        if not index_str.isdigit():
            print("Not a number!\n")
            continue
        index = int(index_str)
        if index < 0 or index > (len(options) - 1):
            print("Index out of range!\n")
            continue
        return options[index]


def run_and_wait_for_key(function_to_run):
    try:
        function_to_run()
    except:
        print("----- ERROR -----")
        traceback.print_exc(file=sys.stdout)
        print()
    finally:
        print("Done!")
        input("Press ENTER to exit...")


if __name__ == "__main__":
    option = select_option(["test1", "test2", "test3"], default=0)
    print(option)
